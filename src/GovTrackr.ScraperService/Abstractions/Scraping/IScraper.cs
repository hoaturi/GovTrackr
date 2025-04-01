namespace GovTrackr.ScraperService.Abstractions.Scraping;

internal interface IScraper
{
    Task ScrapeAsync(List<string> urls, CancellationToken cancellationToken);
}