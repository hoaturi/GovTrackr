using GovTrackr.DocumentScraping.Worker.Infrastructure.Scrapers.Models;
using Shared.MessageContracts;

namespace GovTrackr.DocumentScraping.Worker.Application.Interfaces;

internal interface IScraper
{
    Task<ScrapingResult> ScrapeAsync(List<DocumentInfo> documents, CancellationToken cancellationToken);
}