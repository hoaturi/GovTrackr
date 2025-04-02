namespace GovTrackr.ScraperService.Infrastructure.Scrapers.Models;

internal record ScrapingError(
    string Url,
    string Message
);