using Microsoft.Azure.Functions.Worker;

namespace GovTrackr.DiscoveryService.Abstractions;

internal interface IDiscoveryService
{
    Task DiscoverDocumentsAsync(TimerInfo timerInfo, CancellationToken cancellationToken);
}