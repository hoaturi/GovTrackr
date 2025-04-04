using GovTrackr.DiscoveryService.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovTrackr.DiscoveryService.Services;

internal class DiscoveryService(
    IEnumerable<IDiscoveryStrategy> strategies,
    ILogger<DiscoveryService> logger
) : IDiscoveryService
{
    [Function("DocumentDiscovery")]
    public async Task DiscoverDocumentsAsync(
        [TimerTrigger("*/30 * * * * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        if (!strategies.Any())
        {
            logger.LogWarning("No discovery strategies registered.");
            return;
        }

        logger.LogInformation("Starting document discovery using {StrategyCount} strategies.", strategies.Count());

        var discoveryTasks = strategies
            .Select(strategy => ExecuteDiscoveryStrategyAsync(strategy, cancellationToken))
            .ToList();

        await Task.WhenAll(discoveryTasks);

        logger.LogInformation("Finished document discovery cycle.");
    }

    private async Task ExecuteDiscoveryStrategyAsync(
        IDiscoveryStrategy strategy,
        CancellationToken cancellationToken
    )
    {
        var strategyName = strategy.GetType().Name;

        logger.LogInformation("Executing discovery strategy: {StrategyName}", strategyName);

        var result = await strategy.DiscoverAsync(cancellationToken);

        if (result is not null && result.Urls.Count != 0)
            logger.LogInformation("Strategy {StrategyName} discovered {Count} new documents.", strategyName,
                result.Urls.Count);
        else
            logger.LogInformation("Strategy {StrategyName} found no new documents or failed.", strategyName);

        logger.LogInformation("Completed discovery strategy: {StrategyName}", strategyName);
    }
}