namespace GovTrackr.ScrapingService.Infrastructure.Scrapers.Models;

internal record ScrapingError(
    string Url,
    string Message
);