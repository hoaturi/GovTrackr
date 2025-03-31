using Shared.MessageContracts;

namespace GovTrackr.ScraperService.Scraping.Abstractions;

internal interface IScrapingService
{
    Task ScrapeAsync(DocumentDiscovered documentDiscovered, CancellationToken cancellationToken);
}