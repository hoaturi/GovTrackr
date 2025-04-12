using FluentResults;
using GovTrackr.Api.Features.Subscriptions.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Api.Features.Subscriptions.Digest.UpdateDeliveryTime;

public class UpdateDeliveryTimeHandler(AppDbContext dbContext)
    : IRequestHandler<UpdateDeliveryTimeCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateDeliveryTimeCommand request, CancellationToken cancellationToken)
    {
        var subscription = await dbContext.DigestSubscriptions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (subscription is null) return Result.Fail(SubscriptionErrors.SubscriptionNotFound);

        subscription.DeliveryTime = request.Dto.Time;
        subscription.DeliveryFrequency = request.Dto.Frequency;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(Unit.Value);
    }
}