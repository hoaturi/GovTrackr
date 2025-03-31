namespace GovTrackr.ScraperService.Scraping.Abstractions;

internal interface IScraper
{
    Task ScrapeAsync(string url, CancellationToken cancellationToken);
}