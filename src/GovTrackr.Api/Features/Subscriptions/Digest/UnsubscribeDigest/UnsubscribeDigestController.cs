using GovTrackr.Api.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GovTrackr.Api.Features.Subscriptions.Digest.UnsubscribeDigest;

[ApiController]
[Route("subscriptions/digest")]
public class UnsubscribeDigestController(IMediator mediator) : ApiControllerBase
{
    [HttpDelete]
    public async Task<IActionResult> UnsubscribeDigest(
        [FromQuery] UnsubscribeDigestCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return FromResult(result);
    }
}