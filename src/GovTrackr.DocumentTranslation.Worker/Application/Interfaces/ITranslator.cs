using FluentResults;
using GovTrackr.DocumentTranslation.Worker.Application.Dtos;

namespace GovTrackr.DocumentTranslation.Worker.Application.Interfaces;

public interface ITranslator
{
    Task<Result<TranslatedPresidentialActionDto>> TranslateAsync(string title, string content,
        CancellationToken cancellationToken);
}