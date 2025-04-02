using Shared.MessageContracts;

namespace GovTrackr.ScrapingService.Abstractions;

internal interface IScrapingService
{
    Task ScrapeAsync(DocumentDiscovered documentDiscovered, CancellationToken cancellationToken);
}