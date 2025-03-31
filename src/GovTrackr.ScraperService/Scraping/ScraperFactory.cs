using GovTrackr.ScraperService.Scraping.Abstractions;
using GovTrackr.ScraperService.Scraping.Scrapers;
using Shared.Domain.Common;

namespace GovTrackr.ScraperService.Scraping;

internal class ScraperFactory(ServiceProvider serviceProvider) : IScraperFactory
{
    public IScraper GetScraper(DocumentCategoryType categoryCategory)
    {
        return categoryCategory switch
        {
            DocumentCategoryType.PresidentialAction => serviceProvider.GetRequiredService<PresidentialActionScraper>(),
            _ => throw new ArgumentException($"No scraper found for document type: {categoryCategory}")
        };
    }
}