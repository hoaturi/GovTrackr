using Microsoft.Azure.Functions.Worker;

namespace GovTrackr.DocumentDiscovery.Functions.Application.Interfaces;

internal interface IDocumentDiscoveryFunction
{
    Task DiscoverDocumentsAsync(TimerInfo timerInfo, CancellationToken cancellationToken);
}