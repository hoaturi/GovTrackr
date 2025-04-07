using FluentResults;
using Shared.Domain.PresidentialAction;
using Shared.MessageContracts;

namespace GovTrackr.DocumentScraping.Worker.Application.Interfaces;

internal interface IScraper
{
    Task<Result<PresidentialAction>> ScrapeAsync(DocumentInfo documents, CancellationToken cancellationToken);
}