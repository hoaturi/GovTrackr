using FluentResults;
using GovTrackr.Api.Features.Subscriptions.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Subscription;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Api.Features.Subscriptions.Digest.SubscribeToDigest;

public class SubscribeToDigestCommandHandler(AppDbContext dbContext)
    : IRequestHandler<SubscribeToDigestCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SubscribeToDigestCommand request, CancellationToken cancellationToken)
    {
        var isAlreadySubscribed = await dbContext.DigestSubscriptions
            .AnyAsync(x => x.Email == request.Email, cancellationToken);

        if (isAlreadySubscribed)
            return Result.Fail(SubscriptionErrors.EmailAlreadySubscribed);

        var subscription = new DigestSubscription
        {
            Email = request.Email,
            UnsubscribeToken = Guid.NewGuid().ToString("N")
        };

        dbContext.DigestSubscriptions.Add(subscription);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(Unit.Value);
    }
}