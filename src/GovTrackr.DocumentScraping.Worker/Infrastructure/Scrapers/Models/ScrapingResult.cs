using Shared.Domain.PresidentialAction;

namespace GovTrackr.DocumentScraping.Worker.Infrastructure.Scrapers.Models;

internal record ScrapingResult
{
    public List<PresidentialAction> Successful { get; } = [];
    public List<ScrapingError> Failures { get; } = [];
}