using FluentResults;
using MediatR;

namespace GovTrackr.Api.Features.PresidentialAction.GetPresidentialActions;

public record GetPresidentialActionsQuery(
    int? CategoryId,
    int Page
) : IRequest<Result<GetPresidentialActionsResponse>>;