using Microsoft.Playwright;

namespace GovTrackr.ScraperService.Contracts.Html;

internal interface IBrowserService
{
    Task<IPage> GetPageAsync();
    Task ClosePageAsync(IPage page);
}