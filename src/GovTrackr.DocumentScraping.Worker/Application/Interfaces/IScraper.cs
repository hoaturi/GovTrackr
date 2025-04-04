namespace GovTrackr.DocumentScraping.Worker.Application.Interfaces;

internal interface IScraper
{
    Task ScrapeAsync(List<string> urls, CancellationToken cancellationToken);
}