using Microsoft.Playwright;

namespace Shared.Abstractions.Browser;

public interface IBrowserService
{
    Task<IPage> GetPageAsync();
    Task ClosePageAsync(IPage page);
}