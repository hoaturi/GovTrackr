using Shared.MessageContracts;

namespace GovTrackr.ScraperService.Abstractions.Scraping;

internal interface IScrapingService
{
    Task ScrapeAsync(DocumentDiscovered documentDiscovered, CancellationToken cancellationToken);
}