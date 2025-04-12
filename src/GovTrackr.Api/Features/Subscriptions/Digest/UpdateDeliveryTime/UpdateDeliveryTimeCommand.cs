using FluentResults;
using MediatR;

namespace GovTrackr.Api.Features.Subscriptions.Digest.UpdateDeliveryTime;

public record UpdateDeliveryTimeCommand(Guid Id, UpdateDeliveryTimeDto Dto) : IRequest<Result<Unit>>;