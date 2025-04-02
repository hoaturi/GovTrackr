using Shared.Domain.PresidentialAction;

namespace GovTrackr.ScrapingService.Infrastructure.Scrapers.Models;

internal record ScrapingResult
{
    public List<PresidentialAction> Successful { get; } = [];
    public List<ScrapingError> Failures { get; } = [];
}