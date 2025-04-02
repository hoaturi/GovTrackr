namespace GovTrackr.ScraperService.Abstractions;

internal interface IScraper
{
    Task ScrapeAsync(List<string> urls, CancellationToken cancellationToken);
}