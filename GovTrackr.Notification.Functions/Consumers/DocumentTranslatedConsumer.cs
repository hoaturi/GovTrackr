using GovTrackr.Notification.Functions.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Common;
using Shared.MessageContracts;

namespace GovTrackr.Notification.Functions.Consumers;

public class DocumentTranslatedConsumer(IServiceProvider serviceProvider) : IConsumer<DocumentTranslated>
{
    public async Task Consume(ConsumeContext<DocumentTranslated> context)
    {
        var documentId = context.Message.DocumentId;
        var documentCategory = context.Message.DocumentCategory;

        var notificationService = ResolveNotificationService(documentCategory);

        await notificationService.SendNotificationAsync(documentId, context.CancellationToken);
    }

    private INotificationService ResolveNotificationService(DocumentCategoryType documentCategoryType)
    {
        return serviceProvider.GetRequiredKeyedService<INotificationService>(documentCategoryType);
    }
}