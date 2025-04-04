using GovTrackr.DocumentDiscovery.Functions.Application.Interfaces;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovTrackr.DocumentDiscovery.Functions.Functions;

internal class DocumentDocumentDiscoveryFunction(
    IEnumerable<IDocumentDiscoveryStrategy> strategies,
    IPublishEndpoint publishEndpoint,
    ILogger<DocumentDocumentDiscoveryFunction> logger
) : IDocumentDiscoveryFunction
{
    [Function("DocumentDiscovery")]
    public async Task RunAsync(
        [TimerTrigger("*/10 * * * * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        if (!strategies.Any())
        {
            logger.LogError("No document discovery strategies are registered.");
            return;
        }

        logger.LogInformation("Starting document discovery.");

        var discoveryTasks = strategies
            .Select(strategy => ExecuteStrategyAsync(strategy, cancellationToken))
            .ToList();

        await Task.WhenAll(discoveryTasks);

        logger.LogInformation("Document discovery completed.");
    }

    private async Task ExecuteStrategyAsync(
        IDocumentDiscoveryStrategy strategy,
        CancellationToken cancellationToken
    )
    {
        var strategyName = strategy.GetType().Name;

        var result = await strategy.DiscoverDocumentsAsync(cancellationToken);

        if (result?.Documents.Count > 0)
        {
            logger.LogInformation("{StrategyName} discovered and published {Count} new document(s).",
                strategyName, result.Documents.Count);

            await publishEndpoint.Publish(result, cancellationToken);
        }
    }
}