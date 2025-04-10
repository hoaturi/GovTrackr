using FluentResults;
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

        dbContext.PresidentialActionTranslations.Add(new PresidentialActionTranslation
        {
            Title = result.Value.Title,
            Summary = result.Value.Summary,
            Content = result.Value.Details,
            PresidentialActionId = document.Id
        });

        document.TranslationStatus = TranslationStatus.Completed;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private Task<PresidentialAction?> FindDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        return dbContext.PresidentialActions
            .FirstOrDefaultAsync(pa => pa.Id == documentId, cancellationToken);
    }
}