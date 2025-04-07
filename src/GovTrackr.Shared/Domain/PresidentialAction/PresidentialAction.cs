using Shared.Domain.Common;

namespace Shared.Domain.PresidentialAction;

public class PresidentialAction : BaseEntity
{
    public Guid Id { get; set; }
    public int SubCategoryId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string SourceUrl { get; set; }
    public DateTime PublishedAt { get; set; }
    public DocumentSubCategory SubCategory { get; set; } = null!;
    public required TranslationStatus TranslationStatus { get; set; }
    public PresidentialActionTranslation? Translation { get; set; }
}

public enum TranslationStatus
{
    Pending = 1,
    Failed = 2,
    Completed = 3
}