using FluentResults;
using GovTrackr.DocumentScraping.Worker.Application.Dtos;
using GovTrackr.DocumentScraping.Worker.Application.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Common;
using Shared.Domain.PresidentialAction;
using Shared.Infrastructure.Persistence.Context;
using Shared.MessageContracts;

namespace GovTrackr.DocumentScraping.Worker.Consumers;

internal class DocumentDiscoveredConsumer(
    AppDbContext dbContext,
    IServiceProvider serviceProvider,
    IPublishEndpoint endpoint,
    ILogger<DocumentDiscoveredConsumer> logger
) : IConsumer<DocumentDiscovered>
{
    public async Task Consume(ConsumeContext<DocumentDiscovered> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;
        var category = message.DocumentCategory;

        var isDuplicate = await IsDuplicate(
            message.Document,
            category,
            cancellationToken);

        if (isDuplicate)
            return;

        var scraper = GetScraper(category);
        var scrapeResult = await scraper.ScrapeAsync(context.Message.Document, cancellationToken);

        await HandleScrapeResultAsync(scrapeResult, category, cancellationToken);
    }

    private async Task<bool> IsDuplicate(
        DocumentInfo document,
        DocumentCategoryType category,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.PresidentialActions
            .AsNoTracking()
            .AnyAsync(pa => pa.SourceUrl == document.Url, cancellationToken);

        if (exists)
            logger.LogInformation("[{Category}] Document already exists. Skipping scraping. Url: {Url}",
                category.ToString(), document.Url);

        return exists;
    }

    private async Task HandleScrapeResultAsync(
        Result<ScrapedPresidentialActionDto> result,
        DocumentCategoryType category,
        CancellationToken cancellationToken)
    {
        if (result.IsSuccess)
        {
            var documentId = await SaveSuccessfulDocumentAsync(result.Value, category, cancellationToken);

            var message = new DocumentScraped
            {
                DocumentId = documentId,
                DocumentCategory = category
            };
            await endpoint.Publish(message, cancellationToken);

            logger.LogInformation("[{Category}] Document scraped and published. Url: {Url}",
                category.ToString(), result.Value.SourceUrl);
        }
        else
        {
            var error = result.Errors.First();
            error.Metadata.TryGetValue("Url", out var url);

            logger.LogWarning(
                "[{Category}] Failed to scrape document from URL: {Url}. Error: {Error}",
                category.ToString(), url, error.Message);
        }
    }

    private async Task<Guid> SaveSuccessfulDocumentAsync(
        ScrapedPresidentialActionDto dto,
        DocumentCategoryType category,
        CancellationToken cancellationToken)
    {
        var presidentialAction = new PresidentialAction
        {
            Title = dto.Title,
            Content = dto.Content,
            SourceUrl = dto.SourceUrl,
            PublishedAt = dto.PublishedAt,
            SubCategoryId = (int)dto.SubCategory,
            TranslationStatus = TranslationStatus.Pending
        };

        await dbContext.PresidentialActions.AddAsync(presidentialAction, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return presidentialAction.Id;
    }

    private IScraper GetScraper(DocumentCategoryType category)
    {
        return serviceProvider.GetRequiredKeyedService<IScraper>(category);
    }
}