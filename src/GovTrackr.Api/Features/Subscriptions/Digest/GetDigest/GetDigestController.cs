using GovTrackr.Api.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GovTrackr.Api.Features.Subscriptions.Digest.GetDigest;

[ApiController]
[Route("subscriptions/digest/{id:guid}")]
public class GetDigestController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDigest(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetDigestQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        return FromResult(result);
    }
}