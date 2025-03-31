using Shared.Domain.Common;

namespace Shared.MessageContracts;

public record DocumentDiscovered
{
    public List<string> Urls { get; init; } = [];
    public DocumentCategoryType DocumentCategory { get; init; }
}