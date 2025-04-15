namespace GovTrackr.Digest.Functions.Application.Dtos;

public record PresidentialActionDto(
    Guid Id,
    string Title,
    string Summary,
    string SourceUrl,
    DateTime PublishedAt);