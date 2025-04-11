using FluentResults;
using MediatR;

namespace GovTrackr.Api.Features.PresidentialAction.GetPresidentialActions;

public record GetPresidentialActionsQuery(
    string? Category,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page
) : IRequest<Result<GetPresidentialActionsResponse>>;