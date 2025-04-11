using GovTrackr.DocumentScraping.Worker.Application.Interfaces;
using MassTransit;
using Shared.Domain.Common;
using Shared.MessageContracts;

namespace GovTrackr.DocumentScraping.Worker.Consumers;

internal class DocumentDiscoveredConsumer(
    IServiceProvider serviceProvider,
    IPublishEndpoint endpoint,
    ILogger<DocumentDiscoveredConsumer> logger
) : IConsumer<DocumentDiscovered>
{
    public async Task Consume(ConsumeContext<DocumentDiscovered> context)
    {
        var message = context.Message;
        var documentCategory = message.DocumentCategory;
        var scrapingService = ResolveScrapingService(documentCategory);

        var result = await scrapingService.ScrapeAsync(message.Document, context.CancellationToken);

        if (result.IsSuccess)
        {
            var scrapedMessage = new DocumentScraped
            {
                DocumentId = result.Value,
                DocumentCategory = documentCategory
            };
            await endpoint.Publish(scrapedMessage, context.CancellationToken);

            logger.LogInformation("[{Category}] Document scraped and published successfully. Url: {Url}",
                documentCategory.ToString(), message.Document.Url);

            return;
        }

        logger.LogWarning("[{Category}] Failed to scrape document. Url: {Url}. Error: {Error}",
            documentCategory.ToString(), message.Document.Url, result.Errors.First().Message);
    }

    private IScrapingService ResolveScrapingService(DocumentCategoryType documentCategory)
    {
        return serviceProvider.GetRequiredKeyedService<IScrapingService>(documentCategory);
    }
}