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
        [TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        if (!strategies.Any())
        {
            logger.LogWarning("No discovery strategies registered.");
            return;
        }

        logger.LogInformation("Starting document discovery using {StrategyCount} strategies.", strategies.Count());

        var discoveryTasks = strategies
            .Select(strategy => ExecuteStrategyAsync(strategy, cancellationToken))
            .ToList();

        await Task.WhenAll(discoveryTasks);

        logger.LogInformation("Finished document discovery cycle.");
    }

    private async Task ExecuteStrategyAsync(
        IDocumentDiscoveryStrategy strategy,
        CancellationToken cancellationToken
    )
    {
        var strategyName = strategy.GetType().Name;

        logger.LogInformation("Executing discovery strategy: {StrategyName}", strategyName);

        var result = await strategy.DiscoverDocumentsAsync(cancellationToken);

        if (result is not null && result.Documents.Count != 0)
        {
            logger.LogInformation("Strategy {StrategyName} discovered {Count} new documents.", strategyName,
                result.Documents.Count);

            await publishEndpoint.Publish(result, cancellationToken);

            logger.LogInformation("Published {Count} new documents from strategy {StrategyName}.",
                result.Documents.Count,
                strategyName);
        }
        else
        {
            logger.LogInformation("Strategy {StrategyName} found no new documents.", strategyName);
        }

        logger.LogInformation("Completed discovery strategy: {StrategyName}", strategyName);
    }
}