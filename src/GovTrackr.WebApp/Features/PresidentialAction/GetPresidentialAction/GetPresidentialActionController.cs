using GovTrackr.Application.Common.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GovTrackr.Application.Features.PresidentialAction.GetPresidentialAction;

[ApiController]
[Route("presidential-actions/{id:guid}")]
public class GetPresidentialActionController(IMediator mediator) : ApiControllerBase

{
    [HttpGet]
    public async Task<IActionResult> GetPresidentialAction(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPresidentialActionQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        return FromResult(result);
    }
}