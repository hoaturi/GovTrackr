using GovTrackr.DocumentTranslation.Worker.Application.Interfaces;

namespace GovTrackr.DocumentTranslation.Worker.Application.Services;

public class PresidentialActionTranslationService : ITranslationService
{
    public Task TranslateDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}