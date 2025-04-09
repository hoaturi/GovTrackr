using System.Text.Json.Serialization;

namespace GovTrackr.DocumentTranslation.Worker.Infrastructure.Translators.Models;

public class ProcessedOutput
{
    [JsonPropertyName("title")] public required string Title { get; init; }

    [JsonPropertyName("summary")] public required string Summary { get; init; }

    [JsonPropertyName("details")] public required string Details { get; init; }

    [JsonPropertyName("keywords")] public required List<string> Keywords { get; init; }
}