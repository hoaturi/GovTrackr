using Microsoft.Playwright;

namespace Shared.Application.Interfaces;

public interface IBrowserService
{
    Task<IPage> GetPageAsync();
    Task ClosePageAsync(IPage page);
}