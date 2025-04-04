using GovTrackr.DocumentScraping.Worker.Application.Interfaces;
using GovTrackr.DocumentScraping.Worker.Infrastructure.Scrapers.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Common;
using Shared.Domain.PresidentialAction;
using Shared.Infrastructure.Persistence.Context;
using Shared.MessageContracts;

namespace GovTrackr.DocumentScraping.Worker.Services;

internal class DocumentDiscoveredConsumer(
    AppDbContext dbContext,
    IServiceProvider serviceProvider,
    ILogger<DocumentDiscoveredConsumer> logger
) : IConsumer<DocumentDiscovered>
{
    public async Task Consume(ConsumeContext<DocumentDiscovered> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        var categoryName = message.DocumentCategory.ToString();

        if (message.Documents.Count == 0)
        {
            logger.LogWarning("[{Category}] Received discovery message with no documents.", categoryName);
            return;
        }

        var newDocuments = await FilterNewDocumentsAsync(message.Documents, cancellationToken);

        if (newDocuments.Count == 0)
        {
            logger.LogInformation("[{Category}] All {Total} documents already exist. Skipping scraping.",
                categoryName, message.Documents.Count);
            return;
        }

        logger.LogInformation("[{Category}] {NewCount} new document(s) out of {Total} will be scraped.",
            categoryName, newDocuments.Count, message.Documents.Count);

        var scraper = GetScraper(message.DocumentCategory);
        var scrapeResult = await scraper.ScrapeAsync(message.Documents, cancellationToken);

        await HandleScrapeResultAsync(scrapeResult, newDocuments.Count, message.DocumentCategory, cancellationToken);
    }

    private async Task<List<DocumentInfo>> FilterNewDocumentsAsync(
        IReadOnlyCollection<DocumentInfo> incomingDocuments,
        CancellationToken cancellationToken)
    {
        var incomingUrls = incomingDocuments.Select(d => d.Url).ToList();

        var existingUrls = await dbContext.PresidentialActions
            .AsNoTracking()
            .Where(pa => incomingUrls.Contains(pa.SourceUrl))
            .Select(pa => pa.SourceUrl)
            .ToHashSetAsync(cancellationToken);

        return incomingDocuments
            .Where(d => !existingUrls.Contains(d.Url))
            .ToList();
    }

    private async Task HandleScrapeResultAsync(
        ScrapingResult result,
        int attemptedCount,
        DocumentCategoryType category,
        CancellationToken cancellationToken)
    {
        await SaveSuccessfulDocumentsAsync(result.Successful, category, cancellationToken);
        LogScrapeFailures(result.Failures, attemptedCount, category);
    }

    private async Task SaveSuccessfulDocumentsAsync(
        List<PresidentialAction> successfulDocuments,
        DocumentCategoryType category,
        CancellationToken cancellationToken)
    {
        var successfulSaves = 0;
        var failedSaves = 0;
        var failedDetails = new List<(string Url, string Error)>();

        foreach (var document in successfulDocuments)
            try
            {
                await dbContext.PresidentialActions.AddAsync(document, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                successfulSaves++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{Category}] Failed to save document from URL: {Url}", category,
                    document.SourceUrl);
                failedSaves++;
                failedDetails.Add((document.SourceUrl, ex.Message));
            }
            finally
            {
                dbContext.Entry(document).State = EntityState.Detached;
            }

        if (successfulSaves > 0)
            logger.LogInformation("[{Category}] Successfully saved {SuccessCount} document(s).",
                category, successfulSaves);

        if (failedSaves > 0)
            logger.LogWarning("[{Category}] Failed to save {FailureCount} document(s). Details: {@Failures}",
                category, failedSaves, failedDetails);
    }

    private void LogScrapeFailures(
        List<ScrapingError> failures,
        int totalAttempted,
        DocumentCategoryType category)
    {
        if (failures.Count == 0) return;

        logger.LogWarning(
            "[{Category}] Scraping failed for {FailureCount} of {TotalCount} document(s).",
            category, failures.Count, totalAttempted);
    }

    private IScraper GetScraper(DocumentCategoryType category)
    {
        return serviceProvider.GetRequiredKeyedService<IScraper>(category);
    }
}