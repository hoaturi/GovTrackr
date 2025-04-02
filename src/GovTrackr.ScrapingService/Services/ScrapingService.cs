using GovTrackr.ScrapingService.Abstractions;
using Shared.Domain.Common;
using Shared.MessageContracts;

namespace GovTrackr.ScrapingService.Services;

internal class ScrapingService(
    IServiceProvider serviceProvider
) : BackgroundService, IScrapingService
{

    public async Task ScrapeAsync(DocumentDiscovered message, CancellationToken cancellationToken)
    {
        var documentType = message.DocumentCategory;

        using var scope = serviceProvider.CreateScope();
        var scraper = GetScraper(documentType, scope.ServiceProvider);

        await scraper.ScrapeAsync(message.Urls, cancellationToken);
    }

    private static IScraper GetScraper(DocumentCategoryType categoryCategory, IServiceProvider scopedProvider)
    {
        return scopedProvider.GetRequiredKeyedService<IScraper>(categoryCategory);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await ScrapeAsync(new DocumentDiscovered
        {
            DocumentCategory = DocumentCategoryType.PresidentialAction,
            Urls =
            [
                "https://www.whitehouse.gov/presidential-actions/2025/03/making-the-district-of-columbia-safe-and-beautiful/",
                "https://www.whitehouse.gov/presidential-actions/2025/03/immediate-declassification-of-materials-related-to-the-federal-bureau-of-investigations-crossfire-hurricane-investigation/",
                "https://www.whitehouse.gov/presidential-actions/2025/03/dsada/"
            ]
        }, cancellationToken);
    }
}