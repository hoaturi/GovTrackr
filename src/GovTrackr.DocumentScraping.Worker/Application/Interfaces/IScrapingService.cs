using FluentResults;
using Shared.MessageContracts;

namespace GovTrackr.DocumentScraping.Worker.Application.Interfaces;

public interface IScrapingService
{
    Task<Result<Guid>> ScrapeAsync(
        DocumentInfo document,
        CancellationToken cancellationToken);
}