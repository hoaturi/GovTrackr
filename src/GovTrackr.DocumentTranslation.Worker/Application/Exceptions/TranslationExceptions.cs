namespace GovTrackr.DocumentTranslation.Worker.Application.Exceptions;

public class TranslationException(string message) : Exception(message);

public static class TranslationExceptions
{
    public static readonly TranslationException EmptyResponseContent =
        new("Response content is empty.");

    public static readonly TranslationException InvalidTextContent =
        new("Response does not contain valid text content.");
}