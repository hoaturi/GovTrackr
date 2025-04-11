using FluentResults;
using GovTrackr.DocumentTranslation.Worker.Application.Dtos;
using GovTrackr.DocumentTranslation.Worker.Application.Interfaces;
using GovTrackr.DocumentTranslation.Worker.Infrastructure.Translators.Models;
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
            return Result.Fail(new TranslationError("Presidential action not found"));

        if (document.TranslationStatus == TranslationStatus.Completed)
            return Result.Fail(new TranslationError("Presidential action already translated"));

        var result = await translator.TranslateAsync(document.Title, document.Content, cancellationToken);

        if (!result.IsSuccess) return Result.Fail(new TranslationError(result.Errors.First().Message));

        await SaveTranslatedDocumentAsync(document, result.Value, cancellationToken);

        return Result.Ok(true);
    }

    private async Task SaveTranslatedDocumentAsync(
        PresidentialAction document,
        TranslatedPresidentialActionDto dto,
        CancellationToken cancellationToken)
    {
        var keywordIds = await GetOrCreateKeywordIdsAsync(dto.Keywords, cancellationToken);

        dbContext.PresidentialActionTranslations.Add(new PresidentialActionTranslation
        {
            Title = dto.Title,
            Summary = dto.Summary,
            Content = dto.Details,
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
        var trimmed = keywords.Select(k => k.Trim()).Distinct().ToList();

        var existing = await dbContext.Keywords
            .AsNoTracking()
            .Where(k => trimmed.Contains(k.Name))
            .ToListAsync(cancellationToken);

        var existingMap = existing.ToDictionary(k => k.Name, k => k.Id);

        var newKeywords = trimmed
            .Where(k => !existingMap.ContainsKey(k))
            .Select(k => new Keyword { Name = k })
            .ToList();

        if (newKeywords.Count <= 0) return existingMap.Values.Concat(newKeywords.Select(k => k.Id)).ToList();

        dbContext.Keywords.AddRange(newKeywords);
        await dbContext.SaveChangesAsync(cancellationToken);

        return existingMap.Values.Concat(newKeywords.Select(k => k.Id)).ToList();
    }
}