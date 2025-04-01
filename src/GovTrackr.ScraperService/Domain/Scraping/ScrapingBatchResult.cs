using Shared.Domain.PresidentialAction;

namespace GovTrackr.ScraperService.Domain.Scraping;

internal record ScrapingBatchResult
{
    public List<PresidentialAction> Successful { get; } = [];
    public List<ScrapingError> Failures { get; } = [];
}