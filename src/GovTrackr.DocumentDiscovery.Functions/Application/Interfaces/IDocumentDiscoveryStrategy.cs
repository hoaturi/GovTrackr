using GovTrackr.DocumentDiscovery.Functions.Infrastructure.Strategies.Models;

namespace GovTrackr.DocumentDiscovery.Functions.Application.Interfaces;

internal interface IDocumentDiscoveryStrategy
{
    Task<DiscoveryResult> DiscoverDocumentsAsync(CancellationToken cancellationToken);
}