using FluentResults;
using MediatR;
using Shared.Domain.Notification;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Api.Features.Notifications.RegisterPushPreferences;

public class RegisterPushPreferencesCommandHandler(AppDbContext dbContext)
    : IRequestHandler<RegisterPushPreferencesCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RegisterPushPreferencesCommand request, CancellationToken cancellationToken)
    {
        var token = new PushToken
        {
            Token = request.Token
        };

        foreach (var subscription in request.DocumentType.Select(topic => new PushPreference
                 {
                     Token = token,
                     SubCategoryId = (int)topic
                 }))
            token.Preferences.Add(subscription);

        dbContext.PushTokens.Add(token);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(Unit.Value);
    }
}