using FluentResults;

namespace GovTrackr.DocumentTranslation.Worker.Application.Interfaces;

public interface ITranslationService
{
    Task<Result<bool>> TranslateDocumentAsync(Guid documentId, CancellationToken cancellationToken);
}