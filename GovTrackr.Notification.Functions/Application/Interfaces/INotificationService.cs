using FluentResults;

namespace GovTrackr.Notification.Functions.Application.Interfaces;

public interface INotificationService
{
    Task<Result> SendNotificationAsync(Guid documentId, CancellationToken cancellationToken);
}