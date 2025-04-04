using Shared.MessageContracts;

namespace GovTrackr.DiscoveryService.Abstractions;

internal interface IDiscoveryStrategy
{
    Task<DocumentDiscovered?> DiscoverAsync(CancellationToken cancellationToken);
}