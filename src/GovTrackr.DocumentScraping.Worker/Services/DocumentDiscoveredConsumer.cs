using FluentResults;
using GovTrackr.DocumentScraping.Worker.Application.Dtos;
using GovTrackr.DocumentScraping.Worker.Application.Interfaces;
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
        Result<ScrapedPresidentialActionDto> dto,
        DocumentCategoryType category,
        CancellationToken cancellationToken)
    {
        if (dto.IsSuccess)
        {
            await SaveSuccessfulDocumentAsync(dto.Value, category, cancellationToken);
        }
        else
        {
            var error = dto.Errors.First();
            error.Metadata.TryGetValue("Url", out var url);

            logger.LogWarning(
                "[{Category}] Failed to scrape document from URL: {Url}. Error: {Error}",
                category.ToString(), url, error.Message);
        }
    }

    private async Task SaveSuccessfulDocumentAsync(
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

        logger.LogInformation("[{Category}] Successfully saved document from URL: {Url}",
            category.ToString(), dto.SourceUrl);
    }

    private IScraper GetScraper(DocumentCategoryType category)
    {
        return serviceProvider.GetRequiredKeyedService<IScraper>(category);
    }
}