namespace GovTrackr.DocumentScraping.Worker.Application.Errors;

public class ScrapingException(string message) : Exception(message);

public static class ScrapingExceptions
{
    public static readonly ScrapingException NullResponse = new("Response was null");

    public static ScrapingException TransientHttpError(int status)
    {
        return new ScrapingException($"Transient HTTP error: {status}");
    }
}