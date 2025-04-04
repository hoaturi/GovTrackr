using Shared.MessageContracts;

namespace GovTrackr.DocumentDiscovery.Functions.Application.Interfaces;

internal interface IDiscoveryStrategy
{
    Task<DocumentDiscovered?> DiscoverAsync(CancellationToken cancellationToken);
}