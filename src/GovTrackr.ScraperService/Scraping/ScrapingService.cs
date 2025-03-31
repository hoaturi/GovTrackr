using GovTrackr.ScraperService.Scraping.Abstractions;
using Shared.MessageContracts;

namespace GovTrackr.ScraperService.Scraping;

internal class ScrapingService(
    IScraperFactory scraperFactory
) : IScrapingService
{

    public async Task ScrapeAsync(DocumentDiscovered message, CancellationToken cancellationToken)
    {
        var documentType = message.DocumentCategory;

        var scraper = scraperFactory.GetScraper(documentType);

        foreach (var url in message.Urls) await scraper.ScrapeAsync(url, cancellationToken);
    }
}