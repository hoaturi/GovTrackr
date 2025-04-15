using GovTrackr.Digest.Functions.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;

namespace GovTrackr.Digest.Functions.Functions;

public class DigestFunctions(IDigestEmailService digestEmailService, IDigestMarkdownService digestMarkdownService)
{
    [Function("SendDigestEmailFunction")]
    public async Task SendDigestEmailAsync([TimerTrigger("*/10 * * * * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        await digestEmailService.SendDigestEmailAsync(cancellationToken);
    }

    [Function("CreateDigestFunction")]
    [FixedDelayRetry(5, "00:00:10")]
    // TODO: Change the schedule for production
    public async Task CreateDigestAsync([TimerTrigger("0/10 * * * * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        await digestMarkdownService.CreateDigestAsync(cancellationToken);
    }
}