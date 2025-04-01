using GovTrackr.ScraperService.Scraping.Models;
using Shared.Domain.PresidentialAction;

namespace GovTrackr.ScraperService.Scraping;

internal record ScrapingBatchResult
{
    public List<PresidentialAction> Successful { get; } = [];
    public List<ScrapingError> Failures { get; } = [];
}