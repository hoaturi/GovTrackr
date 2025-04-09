using System.Text.Json.Serialization;

namespace GovTrackr.DocumentTranslation.Worker.Infrastructure.Translators.Models;

public class GeminiRequest
{
    // [JsonPropertyName("system_instruction")]
    // public required List<ContentItem> SystemInstruction { get; set; }

    [JsonPropertyName("contents")] public required List<ContentItem> Contents { get; set; }

    [JsonPropertyName("generationConfig")] public required GenerationConfig GenerationConfig { get; set; }
}

public class ContentItem
{
    [JsonPropertyName("parts")] public required List<Part> Parts { get; set; }
}

public class Part
{
    [JsonPropertyName("text")] public required string Text { get; set; }
}

public class GenerationConfig
{
    [JsonPropertyName("temperature")] public double Temperature { get; set; }
}