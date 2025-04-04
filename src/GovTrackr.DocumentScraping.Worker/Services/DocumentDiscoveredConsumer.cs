using GovTrackr.DocumentScraping.Worker.Application.Interfaces;
using MassTransit;
using Shared.Domain.Common;
using Shared.MessageContracts;

namespace GovTrackr.DocumentScraping.Worker.Services;

internal class DocumentDiscoveredConsumer(
    IServiceProvider serviceProvider,
    ILogger<DocumentDiscoveredConsumer> logger
) : IConsumer<DocumentDiscovered>
{

    public async Task Consume(ConsumeContext<DocumentDiscovered> context)
    {
        var message = context.Message;

        if (message.Documents.Count == 0)
        {
            logger.LogWarning("Received DocumentDiscovered message with no URLs.");
            return;
        }

        var documentType = message.DocumentCategory;

        logger.LogInformation("Received DocumentDiscovered message for {DocumentType} with {UrlCount} URLs.",
            documentType, message.Documents.Count);

        var scraper = GetScraper(documentType, serviceProvider);

        logger.LogInformation("Executing scraper {ScraperType} for category {Category}.",
            scraper.GetType().Name, message.DocumentCategory);

        await scraper.ScrapeAsync(message.Documents, context.CancellationToken);

        logger.LogInformation("Successfully processed DocumentDiscovered message for category {Category}.",
            message.DocumentCategory);
    }

    private static IScraper GetScraper(DocumentCategoryType categoryCategory, IServiceProvider scopedProvider)
    {
        return scopedProvider.GetRequiredKeyedService<IScraper>(categoryCategory);
    }
}