using GovTrackr.Api.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GovTrackr.Api.Features.Subscriptions.Digest.Subscribe;

[ApiController]
[Route("subscriptions/digest")]
public class SubscribeToDigestController(IMediator mediator) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SubscribeToDigest(
        [FromBody] SubscribeToDigestCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return FromResult(result);
    }
}