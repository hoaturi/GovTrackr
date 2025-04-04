namespace GovTrackr.DocumentScraping.Worker.Infrastructure.Scrapers.Models;

internal record ScrapingError(
    string Url,
    string Message
);