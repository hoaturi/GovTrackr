namespace GovTrackr.ScraperService.Domain.Scraping;

internal record ScrapingError(
    string Url,
    string Message
);