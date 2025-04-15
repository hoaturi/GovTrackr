namespace GovTrackr.Digest.Functions.Application.Dtos;

public record DigestEmailTemplateDto(
    List<PresidentialActionDto> PresidentialActions,
    DateTime StartDate,
    DateTime TodayDate
);