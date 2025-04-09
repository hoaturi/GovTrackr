using System.ComponentModel.DataAnnotations;

namespace GovTrackr.DocumentTranslation.Worker.Configurations.Options;

internal class GeminiOptions
{
    internal const string SectionName = "Gemini";

    [Required] public string ApiKey { get; init; } = null!;
}