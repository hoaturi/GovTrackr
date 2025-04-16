using Shared.Domain.Common;

namespace Shared.MessageContracts;

public record DocumentTranslated
{
    public Guid DocumentId { get; init; }

    public DocumentCategoryType DocumentCategory { get; init; }
}