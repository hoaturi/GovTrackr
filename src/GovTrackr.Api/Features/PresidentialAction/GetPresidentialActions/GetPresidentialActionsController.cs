using GovTrackr.Api.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GovTrackr.Api.Features.PresidentialAction.GetPresidentialActions;

[ApiController]
[Route("presidential-actions")]
public class PresidentialActionsController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPresidentialActions(
        [FromQuery] int? categoryId,
        [FromQuery] int page = 1
        , CancellationToken cancellationToken = default)
    {
        var query = new GetPresidentialActionsQuery(categoryId, page);
        var result = await mediator.Send(query, cancellationToken);

        return FromResult(result);
    }
}