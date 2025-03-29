namespace GovTrackr.Application.Features.PresidentialAction.GetPresidentialAction;

public record GetPresidentialActionResponse(
    Guid Id,
    string Category,
    string SubCategory,
    string Title,
    string Content,
    DateTime PublishedAt,
    string Summary,
    string SourceUrl
);