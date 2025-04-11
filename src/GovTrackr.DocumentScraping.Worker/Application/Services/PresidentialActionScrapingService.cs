using FluentResults;
using GovTrackr.DocumentScraping.Worker.Application.Dtos;
using GovTrackr.DocumentScraping.Worker.Application.Errors;
using GovTrackr.DocumentScraping.Worker.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Common;
using Shared.Domain.PresidentialAction;
using Shared.Infrastructure.Persistence.Context;
using Shared.MessageContracts;

namespace GovTrackr.DocumentScraping.Worker.Application.Services;

public class PresidentialActionScrapingService(
    AppDbContext dbContext,
    [FromKeyedServices(DocumentCategoryType.PresidentialAction)]
    IScraper scraper
) : IScrapingService
{

    public async Task<Result<Guid>> ScrapeAsync(DocumentInfo document, CancellationToken cancellationToken)
    {
        if (await IsDuplicateDocumentAsync(document, cancellationToken))
            return Result.Fail(ScrapingErrors.DocumentAlreadyScraped);

        var scrapeResult = await scraper.ScrapeAsync(document, cancellationToken);

        if (scrapeResult.IsSuccess)
            return await SaveScrapedDocumentAsync(scrapeResult.Value, cancellationToken);

        return Result.Fail(scrapeResult.Errors.First());
    }

    private async Task<bool> IsDuplicateDocumentAsync(
        DocumentInfo document,
        CancellationToken cancellationToken)
    {
        return await dbContext.PresidentialActions
            .AnyAsync(pa => pa.SourceUrl == document.Url, cancellationToken);
    }

    private async Task<Guid> SaveScrapedDocumentAsync(
        ScrapedPresidentialActionDto dto,
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
}