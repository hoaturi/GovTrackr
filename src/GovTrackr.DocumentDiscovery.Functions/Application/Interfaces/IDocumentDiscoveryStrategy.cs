using Shared.MessageContracts;

namespace GovTrackr.DocumentDiscovery.Functions.Application.Interfaces;

internal interface IDocumentDiscoveryStrategy
{
    Task<DocumentDiscovered?> DiscoverDocumentsAsync(CancellationToken cancellationToken);
}