using GovTrackr.DocumentDiscovery.Functions.Application.Errors;

namespace GovTrackr.DocumentDiscovery.Functions.Application.Interfaces;

internal interface IDocumentDiscoveryStrategy
{
    Task<DiscoveryResult> DiscoverDocumentsAsync(CancellationToken cancellationToken);
}