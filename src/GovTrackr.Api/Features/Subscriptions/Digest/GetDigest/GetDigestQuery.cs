using FluentResults;
using MediatR;

namespace GovTrackr.Api.Features.Subscriptions.Digest.GetDigest;

public record GetDigestQuery(Guid Id) : IRequest<Result<GetDigestResponse>>;