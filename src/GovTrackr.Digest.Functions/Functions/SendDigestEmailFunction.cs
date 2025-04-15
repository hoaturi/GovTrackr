using GovTrackr.Digest.Functions.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;

namespace GovTrackr.Digest.Functions.Functions;

public class SendDigestEmailFunction(IDigestService digestService)
{
    [Function("SendDigestEmailFunction")]
    public async Task RunAsync([TimerTrigger("*/10 * * * * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        await digestService.SendDigestEmailAsync(cancellationToken);
    }
}