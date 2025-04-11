namespace GovTrackr.DocumentTranslation.Worker.Application.Dtos;

public record TranslatedPresidentialActionDto(
    string Title,
    string Summary,
    string Details,
    List<string> Keywords
);