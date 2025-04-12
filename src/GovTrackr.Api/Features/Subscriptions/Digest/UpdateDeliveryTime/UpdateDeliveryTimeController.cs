using GovTrackr.Api.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GovTrackr.Api.Features.Subscriptions.Digest.UpdateDeliveryTime;

[ApiController]
[Route("subscriptions/digest/{id:guid}")]
public class UpdateDeliveryTimeController(IMediator mediator) : ApiControllerBase
{
    [HttpPut("delivery-time")]
    public async Task<IActionResult> UpdateDeliveryTime(
        Guid id,
        [FromBody] UpdateDeliveryTimeDto dto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDeliveryTimeCommand(id, dto);
        var result = await mediator.Send(command, cancellationToken);

        return FromResult(result);
    }
}