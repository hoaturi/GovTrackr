using Microsoft.Playwright;

namespace GovTrackr.ScraperService.Abstractions.HtmlProcessing;

internal interface IPlaywrightService
{
    Task<IPage> GetPageAsync();
    Task ClosePageAsync(IPage page);
}