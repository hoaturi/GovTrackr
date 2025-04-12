using FluentResults;
using GovTrackr.Api.Features.Subscriptions.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Api.Features.Subscriptions.Digest.Unsubscribe;

public class UnsubscribeDigestCommandHandler(AppDbContext dbContext)
    : IRequestHandler<UnsubscribeDigestCommand, Result<Unit>>
{

    public async Task<Result<Unit>> Handle(UnsubscribeDigestCommand request, CancellationToken cancellationToken)
    {
        var subscription = await dbContext.DigestSubscriptions
            .FirstOrDefaultAsync(x => x.UnsubscribeToken == request.Token, cancellationToken);

        if (subscription is null) return Result.Fail(SubscriptionErrors.SubscriptionNotFound);

        dbContext.DigestSubscriptions.Remove(subscription);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(Unit.Value);
    }
}