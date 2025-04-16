using GovTrackr.Api.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GovTrackr.Api.Features.Notifications.RegisterPushPreferences;

[ApiController]
[Route("api/push")]
public class RegisterPushPreferencesCommandController(IMediator mediator) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RegisterPushPreferences(
        [FromBody] RegisterPushPreferencesCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return FromResult(result);
    }
}