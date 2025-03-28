using GovTrackr.Application.Domain.Common;

namespace GovTrackr.Application.Domain.PresidentialAction;

public class DocumentClassification : BaseEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public required DocumentType Type { get; set; }
}

public enum DocumentType
{
    PresidentialAction = 1
}