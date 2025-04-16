using GovTrackr.Notification.Functions.Application.Dtos;

namespace GovTrackr.Notification.Functions.Application.Interfaces;

public interface IPushService
{
    Task NotifyAsync(NotificationDto dto, CancellationToken cancellationToken);
}