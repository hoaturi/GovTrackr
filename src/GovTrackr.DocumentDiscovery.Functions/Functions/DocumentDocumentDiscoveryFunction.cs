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
) 
{
    [Function("DocumentDiscovery")]
    public async Task RunAsync(
        [TimerTrigger("*/30 * * * * *")] TimerInfo timerInfo,
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

        var totalCount = result.DiscoveredDocuments.Count;

        if (totalCount > 0)
        {
            var successCount = 0;

            foreach (var discoveredDocument in result.DiscoveredDocuments.Select(document => new DocumentDiscovered
                     {
                         DocumentCategory = result.DocumentCategory,
                         Document = document
                     }))
                try
                {
                    await publishEndpoint.Publish(discoveredDocument, cancellationToken);
                    successCount++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "[{StrategyName}] Failed to publish document: {Title}",
                        strategyName, discoveredDocument.Document.Title);
                }

            logger.LogInformation("[{StrategyName}] Successfully published {SuccessCount}/{TotalCount} document(s).",
                strategyName, successCount, totalCount);
        }
        else
        {
            logger.LogInformation("[{StrategyName}] No new documents discovered.", strategyName);
        }
    }
}