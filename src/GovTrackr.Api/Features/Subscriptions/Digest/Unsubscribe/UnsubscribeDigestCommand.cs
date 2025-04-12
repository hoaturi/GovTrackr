using FluentResults;
using MediatR;

namespace GovTrackr.Api.Features.Subscriptions.Digest.Unsubscribe;

public record UnsubscribeDigestCommand(string Token) : IRequest<Result<Unit>>;