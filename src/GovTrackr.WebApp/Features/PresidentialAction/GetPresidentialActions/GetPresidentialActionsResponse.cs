using GovTrackr.Application.Domain.Common;
using GovTrackr.Application.Domain.PresidentialAction;

namespace GovTrackr.Application.Features.PresidentialAction.GetPresidentialActions;

public record GetPresidentialActionsResponse(
    List<GetPresidentialActionsItem> Items,
    int TotalCount, 
    int Page
);

public record GetPresidentialActionsItem(
    Guid Id,
    string Title,
    string Summary,
    string SourceTitle,
    string SourceUrl,
    DateTime PublishedAt,
    DocumentSubCategory SubCategory
);