using System.Net;
using Shared.Abstractions.Errors;

namespace GovTrackr.DocumentTranslation.Worker.Application.Errors;

public sealed class TranslationError(string message) : ApplicationError(message);

public static class TranslationErrors
{
    public static readonly TranslationError DocumentNotFound = new("Document not found");

    public static readonly TranslationError DocumentAlreadyTranslated =
        new("Document already translated");

    public static TranslationError ApiFailed(HttpStatusCode statusCode)
    {
        return new TranslationError($"Translation API failed. Status code: {(int)statusCode} ({statusCode})");
    }
}