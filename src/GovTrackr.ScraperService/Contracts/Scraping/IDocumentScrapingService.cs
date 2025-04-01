using Shared.MessageContracts;

namespace GovTrackr.ScraperService.Contracts.Scraping;

internal interface IDocumentScrapingService
{
    Task ScrapeAsync(DocumentDiscovered documentDiscovered, CancellationToken cancellationToken);
}