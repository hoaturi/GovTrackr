using FluentResults;
using MediatR;

namespace GovTrackr.Api.Features.PresidentialAction.GetPresidentialAction;

public record GetPresidentialActionQuery(Guid Id) : IRequest<Result<GetPresidentialActionResponse>>;