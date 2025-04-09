using FluentResults;
using GovTrackr.DocumentScraping.Worker.Application.Dtos;
using Shared.MessageContracts;

namespace GovTrackr.DocumentScraping.Worker.Application.Interfaces;

public interface IScraper
{
    Task<Result<ScrapedPresidentialActionDto>> ScrapeAsync(DocumentInfo documents, CancellationToken cancellationToken);
}