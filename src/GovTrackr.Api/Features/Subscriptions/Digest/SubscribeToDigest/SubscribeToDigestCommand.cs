using FluentResults;
using MediatR;

namespace GovTrackr.Api.Features.Subscriptions.Digest.SubscribeToDigest;

public record SubscribeToDigestCommand(string Email)
    : IRequest<Result<Unit>>;