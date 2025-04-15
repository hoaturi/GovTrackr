using GovTrackr.Digest.Functions.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;

namespace GovTrackr.Digest.Functions.Functions;

public class BuildDigestMarkdownFunction(IDigestContentBuilder contentBuilder)
{
    [Function("BuildDigestMarkdownFunction")]
    [FixedDelayRetry(5, "00:00:10")]
    public async Task RunAsync([TimerTrigger("0/30 * * * * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        await contentBuilder.BuildMarkdownDigestContentAsync(cancellationToken);
    }
}