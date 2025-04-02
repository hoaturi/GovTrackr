using Shared.MessageContracts;

namespace GovTrackr.ScraperService.Abstractions;

internal interface IDocumentScrapingService
{
    Task ScrapeAsync(DocumentDiscovered documentDiscovered, CancellationToken cancellationToken);
}