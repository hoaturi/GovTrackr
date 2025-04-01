using GovTrackr.ScraperService.Abstractions.HtmlProcessing;
using Microsoft.Playwright;

namespace GovTrackr.ScraperService.Services.HtmlProcessing;

public class PlaywrightService : IPlaywrightService
{
    private readonly IPage _page;
    private readonly IPlaywright _playwright = Playwright.CreateAsync().GetAwaiter().GetResult();

    public PlaywrightService()
    {
        var browser = _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        }).GetAwaiter().GetResult();

        _page = browser.NewPageAsync().GetAwaiter().GetResult();
    }

    public Task<IPage> GetPageAsync()
    {
        return Task.FromResult(_page);
    }

    public async Task ClosePageAsync()
    {
        await _page.CloseAsync();
    }
}