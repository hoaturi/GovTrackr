using Shared.Domain.Common;

namespace Shared.MessageContracts;

public record DocumentScraped
{
    public Guid DocumentId { get; init; }

    public DocumentCategoryType DocumentCategory { get; init; }
}