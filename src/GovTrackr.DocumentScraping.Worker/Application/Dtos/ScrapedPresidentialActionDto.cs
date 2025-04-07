using Shared.Domain.Common;

namespace GovTrackr.DocumentScraping.Worker.Application.Dtos;

public record ScrapedPresidentialActionDto(
    string Title,
    string Content,
    string SourceUrl,
    DateTime PublishedAt,
    DocumentSubCategoryType SubCategory
);