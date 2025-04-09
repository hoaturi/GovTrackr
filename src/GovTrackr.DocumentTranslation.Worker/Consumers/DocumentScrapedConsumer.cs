using GovTrackr.DocumentTranslation.Worker.Application.Interfaces;
using MassTransit;
using Shared.Domain.Common;
using Shared.MessageContracts;

namespace GovTrackr.DocumentTranslation.Worker.Consumers;

public class DocumentScrapedConsumer(
    IServiceProvider serviceProvider
) : IConsumer<DocumentScraped>
{
    public async Task Consume(ConsumeContext<DocumentScraped> context)
    {
        var documentId = context.Message.DocumentId;
        var documentCategory = context.Message.DocumentCategory;

        var translationService = GetTranslationService(documentCategory);

        await translationService.TranslateDocumentAsync(documentId, context.CancellationToken);
    }

    private ITranslationService GetTranslationService(DocumentCategoryType category)
    {
        var service = serviceProvider.GetRequiredKeyedService<ITranslationService>(category);

        return service;
    }
}