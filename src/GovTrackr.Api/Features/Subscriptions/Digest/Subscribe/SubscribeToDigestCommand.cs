using FluentResults;
using MediatR;
using Shared.Domain.Subscription;

namespace GovTrackr.Api.Features.Subscriptions.Digest.Subscribe;

public record SubscribeToDigestCommand(string Email, DeliveryTime Time, DeliveryFrequency Frequency)
    : IRequest<Result<Unit>>;