using FluentResults;
using GovTrackr.DocumentTranslation.Worker.Application;
using GovTrackr.DocumentTranslation.Worker.Application.Interfaces;

namespace GovTrackr.DocumentTranslation.Worker.Infrastructure.Translators;

public class PresidentialActionTranslator(
    IHttpClientFactory httpClientFactory,
    IPromptProvider promptProvider
) : ITranslator
{

    public async Task<Result<TranslatedPresidentialActionDto>> TranslateAsync(string title, string content,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}