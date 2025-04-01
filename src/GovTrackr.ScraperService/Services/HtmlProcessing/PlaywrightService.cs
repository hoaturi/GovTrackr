using GovTrackr.ScraperService.Abstractions.HtmlProcessing;
using Microsoft.Playwright;

namespace GovTrackr.ScraperService.Services.HtmlProcessing;

public class PlaywrightService : IPlaywrightService, IAsyncDisposable
{
    private readonly Task _initializationTask;
    private IBrowser? _browser;
    private IPlaywright? _playwright;

    public PlaywrightService()
    {
        _initializationTask = InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
            await _browser.CloseAsync();

        _playwright?.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<IPage> GetPageAsync()
    {
        await _initializationTask;
        var context = await _browser!.NewContextAsync();
        return await context.NewPageAsync();
    }

    public async Task ClosePageAsync(IPage page)
    {
        var context = page.Context;
        await page.CloseAsync();
        await context.CloseAsync(); 
    }

    private async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }
}