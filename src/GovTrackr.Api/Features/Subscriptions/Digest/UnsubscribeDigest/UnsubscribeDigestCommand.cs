using FluentResults;
using MediatR;

namespace GovTrackr.Api.Features.Subscriptions.Digest.UnsubscribeDigest;

public record UnsubscribeDigestCommand(string Token) : IRequest<Result<Unit>>;