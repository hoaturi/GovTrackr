using GovTrackr.DocumentDiscovery.Functions.Application.Interfaces;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Shared.MessageContracts;

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
            logger.LogError("No document discovery strategies are registered.");
            return;
        }

        var discoveryTasks = strategies
            .Select(strategy => ExecuteStrategyAsync(strategy, cancellationToken))
            .ToList();

        await Task.WhenAll(discoveryTasks);
    }

    private async Task ExecuteStrategyAsync(
        IDocumentDiscoveryStrategy strategy,
        CancellationToken cancellationToken)
    {
        var strategyName = strategy.GetType().Name;
        logger.LogInformation("Running discovery strategy: {StrategyName}", strategyName);

        var result = await strategy.DiscoverDocumentsAsync(cancellationToken);

        foreach (var error in result.Errors)
            logger.LogWarning("[{StrategyName}] Discovery error at URL: {Url}. Message: {Message}",
                strategyName, error.Url, error.Message);

        if (result.DiscoveredDocuments.Count > 0)
        {
            var discoveredDocuments = new DocumentDiscovered
            {
                DocumentCategory = result.DocumentCategory,
                Documents = result.DiscoveredDocuments
            };

            await publishEndpoint.Publish(discoveredDocuments, cancellationToken);
            logger.LogInformation("[{StrategyName}] Published {Count} new document(s).",
                strategyName, result.DiscoveredDocuments.Count);
        }
        else
        {
            logger.LogInformation("[{StrategyName}] No new documents discovered.", strategyName);
        }
    }
}