using Microsoft.Playwright;
using Shared.Abstractions.Browser;

namespace Shared.Infrastructure.Browser;

public class PlaywrightService : IBrowserService, IAsyncDisposable
{
    private readonly SemaphoreSlim _contextSemaphore = new(10);
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
        _contextSemaphore.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<IPage> GetPageAsync()
    {
        await _initializationTask;
        await _contextSemaphore.WaitAsync();

        var context = await _browser!.NewContextAsync();
        return await context.NewPageAsync();
    }

    public async Task ClosePageAsync(IPage page)
    {
        var context = page.Context;

        try
        {
            await page.CloseAsync();
            await context.CloseAsync();
        }
        finally
        {
            _contextSemaphore.Release();
        }
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