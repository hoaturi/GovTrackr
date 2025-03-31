using GovTrackr.ScraperService.Scraping.Abstractions;
using Shared.Domain.Common;

namespace GovTrackr.ScraperService.Scraping;

internal class ScraperFactory(IServiceProvider serviceProvider) : IScraperFactory
{
    public IScraper GetScraper(DocumentCategoryType categoryCategory)
    {
        return serviceProvider.GetRequiredKeyedService<IScraper>(categoryCategory);
    }
}