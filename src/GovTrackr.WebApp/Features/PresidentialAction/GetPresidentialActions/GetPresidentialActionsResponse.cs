using GovTrackr.Application.Domain.PresidentialAction;

namespace GovTrackr.Application.Features.PresidentialAction.GetPresidentialActions;

public record GetPresidentialActionsResponse(
    List<GetPresidentialActionsItem> Items
);

public record GetPresidentialActionsItem(
    Guid Id,
    string Title,
    string Summary,
    string OriginalTitle,
    string SourceUrl,
    DateTime PublishedAt,
    DocumentClassification Classification
);