namespace GovTrackr.DocumentTranslation.Worker.Application;

public record TranslatedPresidentialActionDto(
    string Title,
    string Summary,
    string Details,
    List<string> Keywords
);