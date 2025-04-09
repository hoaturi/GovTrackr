namespace GovTrackr.DocumentTranslation.Worker.Application.Interfaces;

public interface ITranslationService
{
    Task TranslateDocumentAsync(Guid documentId, CancellationToken cancellationToken);
}