namespace Shared.Domain.Common;

public class DocumentCategory : BaseEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
}

public enum DocumentCategoryType
{
    PresidentialAction = 1
}