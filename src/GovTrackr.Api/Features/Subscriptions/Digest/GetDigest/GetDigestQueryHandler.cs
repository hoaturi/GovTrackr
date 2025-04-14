using FluentResults;
using GovTrackr.Api.Features.Subscriptions.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Api.Features.Subscriptions.Digest.GetDigest;

public class GetDigestQueryHandler(AppDbContext dbContext) : IRequestHandler<GetDigestQuery, Result<GetDigestResponse>>
{
    public async Task<Result<GetDigestResponse>> Handle(GetDigestQuery request, CancellationToken cancellationToken)
    {
        var digest = await dbContext.DigestSubscriptions
            .Where(d => d.Id == request.Id)
            .Select(d => new GetDigestResponse
            {
                LastSentAt = d.LastSentAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return digest is not null
            ? Result.Ok(digest)
            : Result.Fail(SubscriptionErrors.SubscriptionNotFound);
    }
}