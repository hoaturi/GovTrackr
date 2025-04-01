namespace GovTrackr.ScraperService.Contracts.Scraping;

internal interface IScraper
{
    Task ScrapeAsync(List<string> urls, CancellationToken cancellationToken);
}