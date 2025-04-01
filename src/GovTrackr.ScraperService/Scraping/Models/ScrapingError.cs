namespace GovTrackr.ScraperService.Scraping.Models;

internal record ScrapingError(
    string Url,
    string Message
);