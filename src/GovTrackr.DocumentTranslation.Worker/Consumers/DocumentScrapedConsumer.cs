using GovTrackr.DocumentTranslation.Worker.Application.Interfaces;
using MassTransit;
using Shared.Domain.Common;
using Shared.MessageContracts;

namespace GovTrackr.DocumentTranslation.Worker.Consumers;

public class DocumentScrapedConsumer(
    IServiceProvider serviceProvider,
    ILogger<DocumentScrapedConsumer> logger)
    : IConsumer<DocumentScraped>
{
    public async Task Consume(ConsumeContext<DocumentScraped> context)
    {
        var message = context.Message;
        var service = ResolveTranslationService(message.DocumentCategory);

        var result = await service.TranslateDocumentAsync(message.DocumentId, context.CancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("[{Category}] Document translated successfully. Id: {Id}",
                message.DocumentCategory.ToString(), message.DocumentId);
            return;
        }

        logger.LogWarning("[{Category}] Failed to translate document. Id: {Id}. Reason: {Reason}",
            message.DocumentCategory.ToString(), message.DocumentId, result.Errors.First().Message);
    }

    private ITranslationService ResolveTranslationService(DocumentCategoryType category)
    {
        return serviceProvider.GetRequiredKeyedService<ITranslationService>(category);
    }
}