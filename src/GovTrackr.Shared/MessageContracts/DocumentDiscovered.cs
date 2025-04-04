using Shared.Domain.Common;

namespace Shared.MessageContracts;

public record DocumentDiscovered
{
    public List<DocumentInfo> Documents { get; init; } = [];
    public DocumentCategoryType DocumentCategory { get; init; }
}

public record DocumentInfo(string Url, string Title);