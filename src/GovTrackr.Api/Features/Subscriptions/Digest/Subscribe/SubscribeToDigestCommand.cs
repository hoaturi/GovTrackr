using FluentResults;
using MediatR;
using Shared.Domain.Subscription;

namespace GovTrackr.Api.Features.Subscriptions.Digest.Subscribe;

public record SubscribeToDigestCommand(string Email, DeliveryTime DeliveryTime) : IRequest<Result<Unit>>;