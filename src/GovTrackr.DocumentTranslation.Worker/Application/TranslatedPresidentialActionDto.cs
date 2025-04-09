namespace GovTrackr.DocumentTranslation.Worker.Application;

public record TranslatedPresidentialActionDto(
    string Title,
    string Details
);