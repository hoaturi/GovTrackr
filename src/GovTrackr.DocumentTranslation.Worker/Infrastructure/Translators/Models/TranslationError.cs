using FluentResults;

namespace GovTrackr.DocumentTranslation.Worker.Infrastructure.Translators.Models;

internal class TranslationError(string message) : Error(message);