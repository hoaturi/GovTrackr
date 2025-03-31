using Shared.Domain.Common;

namespace GovTrackr.ScraperService.Scraping.Abstractions;

internal interface IScraperFactory
{
    IScraper GetScraper(DocumentCategoryType categoryCategory);
}