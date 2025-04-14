using FluentResults;
using GovTrackr.DocumentTranslation.Worker.Application.Dtos;
using GovTrackr.DocumentTranslation.Worker.Application.Errors;
using GovTrackr.DocumentTranslation.Worker.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Common;
using Shared.Domain.PresidentialAction;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.DocumentTranslation.Worker.Application.Services;

public class PresidentialActionTranslationService(
    AppDbContext dbContext,
    [FromKeyedServices(DocumentCategoryType.PresidentialAction)]
    ITranslator translator)
    : ITranslationService
{
    public async Task<Result<bool>> TranslateDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var document = await dbContext.PresidentialActions
            .FirstOrDefaultAsync(pa => pa.Id == documentId, cancellationToken);

        if (document is null)
            return Result.Fail(TranslationErrors.DocumentNotFound);

        if (document.TranslationStatus == TranslationStatus.Completed)
            return Result.Fail(TranslationErrors.DocumentAlreadyTranslated);

        var result = await translator.TranslateAsync(document.Title, document.Content, cancellationToken);

        if (!result.IsSuccess) return Result.Fail(result.Errors.First());

        await SaveTranslatedDocumentAsync(document, result.Value, cancellationToken);

        return Result.Ok(true);
    }

    private async Task SaveTranslatedDocumentAsync(
        PresidentialAction document,
        TranslatedPresidentialActionDto dto,
        CancellationToken cancellationToken)
    {
        var keywordsToAssociate = await GetOrCreateKeywordsAsync(dto.Keywords, cancellationToken);

        dbContext.PresidentialActionTranslations.Add(new PresidentialActionTranslation
        {
            Title = dto.Title,
            Summary = dto.Summary,
            Content = dto.Details,
            PresidentialActionId = document.Id,
            Keywords = keywordsToAssociate
        });

        document.TranslationStatus = TranslationStatus.Completed;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<Keyword>> GetOrCreateKeywordsAsync(
        List<string> keywordNames,
        CancellationToken cancellationToken)
    {
        if (keywordNames.Count == 0) return [];

        var normalizedKeywords = keywordNames
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrEmpty(k))
            .Distinct()
            .ToList();

        if (normalizedKeywords.Count == 0) return [];

        var existingKeywords = await dbContext.Keywords
            .Where(k => normalizedKeywords.Contains(k.Name))
            .ToListAsync(cancellationToken);

        var existingKeywordNames = existingKeywords.Select(k => k.Name).ToHashSet();

        var newKeywordNames = normalizedKeywords
            .Where(k => !existingKeywordNames.Contains(k))
            .ToList();

        List<Keyword> newKeywords = [];
        if (newKeywordNames.Count <= 0) return existingKeywords.Concat(newKeywords).ToList();

        newKeywords = newKeywordNames.Select(name => new Keyword { Name = name }).ToList();
        dbContext.Keywords.AddRange(newKeywords);

        return existingKeywords.Concat(newKeywords).ToList();
    }
}