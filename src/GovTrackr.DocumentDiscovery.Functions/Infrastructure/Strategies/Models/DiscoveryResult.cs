using Shared.Domain.Common;
using Shared.MessageContracts;

namespace GovTrackr.DocumentDiscovery.Functions.Infrastructure.Strategies.Models;

internal sealed record DiscoveryResult(
    DocumentCategoryType DocumentCategory,
    List<DocumentInfo> DiscoveredDocuments,
    List<DiscoveryError> Errors
);