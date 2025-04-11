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
        [FromQuery] string? category,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1
        , CancellationToken cancellationToken = default)
    {
        var query = new GetPresidentialActionsQuery(category, fromDate, toDate, page);
        var result = await mediator.Send(query, cancellationToken);

        return FromResult(result);
    }
}