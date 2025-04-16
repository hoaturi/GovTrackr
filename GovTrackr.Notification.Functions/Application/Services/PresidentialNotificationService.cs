using FluentResults;
using GovTrackr.Notification.Functions.Application.Interfaces;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Notification.Functions.Application.Services;

public class PresidentialNotificationService(AppDbContext dbContext, IPushService pushService)
    : INotificationService
{
    public async Task<Result> SendNotificationAsync(Guid documentId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}