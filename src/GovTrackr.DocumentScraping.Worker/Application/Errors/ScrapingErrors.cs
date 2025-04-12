using Shared.Common.Errors;

namespace GovTrackr.DocumentScraping.Worker.Application.Errors;

public class ScrapingError(string message) : ResultError(message);

public static class ScrapingErrors
{
    public static readonly ScrapingError DocumentAlreadyScraped =
        new("Document already scraped");

    public static readonly ScrapingError EmptyContent = new("Parsed content is empty");

    public static readonly ScrapingError FailedToParseCategory =
        new("Failed to infer document category");

    public static readonly ScrapingError FailedToParseDate =
        new("Failed to parse document date");

    public static ScrapingError ApiFailed(int status)
    {
        return new ScrapingError($"Scraping API failed. Status code: {status}");
    }
}