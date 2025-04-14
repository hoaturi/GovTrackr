using Shared.Domain.Common;
using Shared.MessageContracts;

namespace GovTrackr.DocumentDiscovery.Functions.Application.Errors;

internal sealed record DiscoveryResult(
    DocumentCategoryType DocumentCategory,
    List<DocumentInfo> DiscoveredDocuments,
    List<DiscoveryError> Errors
);