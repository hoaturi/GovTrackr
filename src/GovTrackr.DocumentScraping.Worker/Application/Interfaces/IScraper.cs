using FluentResults;
using GovTrackr.DocumentScraping.Worker.Application.Dtos;
using Microsoft.Playwright;
using Shared.MessageContracts;

namespace GovTrackr.DocumentScraping.Worker.Application.Interfaces;

public interface IScraper
{
    Task<Result<ScrapedPresidentialActionDto>> ScrapeAsync(DocumentInfo documents, CancellationToken cancellationToken);

    Task<Result<ScrapedPresidentialActionDto>> ParseAsync(IPage page, DocumentInfo document,
        CancellationToken cancellationToken);
}