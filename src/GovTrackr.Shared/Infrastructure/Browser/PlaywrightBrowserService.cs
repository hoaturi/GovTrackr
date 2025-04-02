using Microsoft.Playwright;
using Shared.Abstractions.Browser;

namespace Shared.Infrastructure.Browser;

public class PlaywrightBrowserService : IBrowserService, IAsyncDisposable
{
    private readonly Task _initializationTask;
    private IBrowser? _browser;
    private IPlaywright? _playwright;

    public PlaywrightBrowserService()
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