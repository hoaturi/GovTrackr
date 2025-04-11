using FluentResults;
using GovTrackr.DocumentTranslation.Worker.Application.Dtos;
using GovTrackr.DocumentTranslation.Worker.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Common;
using Shared.Domain.PresidentialAction;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.DocumentTranslation.Worker.Application.Services;

public class PresidentialActionTranslationService(
    AppDbContext dbContext,
    ILogger<ITranslationService> logger,
    [FromKeyedServices(DocumentCategoryType.PresidentialAction)]
    ITranslator translator)
    : ITranslationService
{
    public async Task TranslateDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var document = await FindDocumentAsync(documentId, cancellationToken);
        if (document is null)
        {
            logger.LogWarning("Presidential action with ID {DocumentId} not found.", documentId);
            return;
        }

        if (document.TranslationStatus == TranslationStatus.Completed)
        {
            logger.LogInformation("Presidential action with ID {DocumentId} already translated.", documentId);
            return;
        }

        var result = await translator.TranslateAsync(document.Title, document.Content, cancellationToken);
        await ProcessTranslationResultAsync(document, result, cancellationToken);
    }

    private Task<PresidentialAction?> FindDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        return dbContext.PresidentialActions
            .FirstOrDefaultAsync(pa => pa.Id == documentId, cancellationToken);
    }

    private async Task ProcessTranslationResultAsync(
        PresidentialAction document,
        Result<TranslatedPresidentialActionDto> result,
        CancellationToken cancellationToken)
    {
        if (result.IsFailed)
        {
            var errorMessage = result.Errors.First().Message;
            logger.LogError("Translation failed for document ID {DocumentId}: {Error}", document.Id, errorMessage);
            return;
        }

        var keywordIds = await GetOrCreateKeywordIdsAsync(result.Value.Keywords, cancellationToken);

        dbContext.PresidentialActionTranslations.Add(new PresidentialActionTranslation
        {
            Title = result.Value.Title,
            Summary = result.Value.Summary,
            Content = result.Value.Details,
            PresidentialActionId = document.Id,
            KeywordIds = keywordIds
        });

        document.TranslationStatus = TranslationStatus.Completed;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<int>> GetOrCreateKeywordIdsAsync(
        List<string> keywords,
        CancellationToken cancellationToken)
    {
        var trimmedKeywords = keywords.Select(k => k.Trim()).Distinct().ToList();

        var existingKeywords = await dbContext.Keywords
            .Where(k => trimmedKeywords.Contains(k.Name))
            .ToListAsync(cancellationToken);

        var existingKeywordMap = existingKeywords.ToDictionary(k => k.Name, k => k.Id);

        var newKeywords = trimmedKeywords
            .Where(k => !existingKeywordMap.ContainsKey(k))
            .Select(k => new Keyword { Name = k })
            .ToList();

        if (newKeywords.Count <= 0)
            return existingKeywordMap.Values
                .Concat(newKeywords.Select(k => k.Id))
                .ToList();

        dbContext.Keywords.AddRange(newKeywords);
        await dbContext.SaveChangesAsync(cancellationToken);

        return existingKeywordMap.Values
            .Concat(newKeywords.Select(k => k.Id))
            .ToList();
    }
}