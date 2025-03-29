using FluentResults;
using MediatR;

namespace GovTrackr.Application.Features.PresidentialAction.GetPresidentialAction;

public record GetPresidentialActionQuery(Guid Id) : IRequest<Result<GetPresidentialActionResponse>>;