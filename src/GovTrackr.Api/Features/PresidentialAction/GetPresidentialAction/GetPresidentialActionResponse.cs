namespace GovTrackr.Api.Features.PresidentialAction.GetPresidentialAction;

public record GetPresidentialActionResponse(
    Guid Id,
    int Category,
    int SubCategory,
    string Title,
    string Content,
    DateTime PublishedAt,
    string Summary,
    string SourceUrl
);