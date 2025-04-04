using GovTrackr.DocumentScraping.Worker.Application.Interfaces;
using GovTrackr.DocumentScraping.Worker.Infrastructure.Scrapers.Models;
using MassTransit;
using Shared.Domain.Common;
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

        if (message.Documents.Count == 0)
        {
            logger.LogWarning("Received DocumentDiscovered message with no URLs.");
            return;
        }

        var documentType = message.DocumentCategory;

        logger.LogInformation("Received DocumentDiscovered message for {DocumentType} with {UrlCount} URLs.",
            documentType, message.Documents.Count);

        var scraper = GetScraper(documentType, serviceProvider);

        logger.LogInformation("Executing scraper {ScraperType} for category {Category}.",
            scraper.GetType().Name, message.DocumentCategory);

        var result = await scraper.ScrapeAsync(message.Documents, cancellationToken);

        await HandleScrapeResultAsync(result, message, cancellationToken);

        logger.LogInformation("Finished processing DocumentDiscovered message for category {Category}.",
            message.DocumentCategory);
    }

    private async Task HandleScrapeResultAsync(
        ScrapingResult result,
        DocumentDiscovered message,
        CancellationToken cancellationToken)
    {
        var totalAttempted = message.Documents.Count;

        if (result.Successful.Count > 0)
        {
            await dbContext.PresidentialActions.AddRangeAsync(result.Successful, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Successfully scraped and saved {SuccessCount} of {TotalCount} documents for category {Category}",
                result.Successful.Count, totalAttempted, message.DocumentCategory);
        }
        else
        {
            logger.LogInformation(
                "No documents were successfully scraped out of {TotalCount} attempts for category {Category}",
                totalAttempted, message.DocumentCategory);
        }

        if (result.Failures.Count > 0)
        {
            logger.LogWarning("Failed to scrape {FailureCount} of {TotalCount} documents for category {Category}.",
                result.Failures.Count,
                totalAttempted,
                message.DocumentCategory);

            foreach (var failure in result.Failures)
                logger.LogWarning("Scrape Failure for URL '{Url}': {Reason}", failure.Document.Url, failure.Message);
        }
    }

    private static IScraper GetScraper(DocumentCategoryType categoryCategory, IServiceProvider scopedProvider)
    {
        return scopedProvider.GetRequiredKeyedService<IScraper>(categoryCategory);
    }
}