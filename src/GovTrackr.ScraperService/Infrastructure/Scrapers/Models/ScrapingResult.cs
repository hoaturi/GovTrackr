using Shared.Domain.PresidentialAction;

namespace GovTrackr.ScraperService.Infrastructure.Scrapers.Models;

internal record ScrapingResult
{
    public List<PresidentialAction> Successful { get; } = [];
    public List<ScrapingError> Failures { get; } = [];
}