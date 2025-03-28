using GovTrackr.Application.Domain.Common;

namespace GovTrackr.Application.Domain.PresidentialAction;

public class DocumentSubCategory : BaseEntity
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public DocumentCategory Category { get; set; } = null!;
}

public enum DocumentSubCategoryType
{
    ExecutiveOrder = 1,
    Memoranda = 2,
    Proclamation = 3
}