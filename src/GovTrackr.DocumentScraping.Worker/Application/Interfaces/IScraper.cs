using Shared.MessageContracts;

namespace GovTrackr.DocumentScraping.Worker.Application.Interfaces;

internal interface IScraper
{
    Task ScrapeAsync(List<DocumentInfo> documents, CancellationToken cancellationToken);
}