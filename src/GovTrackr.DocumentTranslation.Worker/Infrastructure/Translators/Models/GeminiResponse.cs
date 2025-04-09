using System.Text.Json.Serialization;

namespace GovTrackr.DocumentTranslation.Worker.Infrastructure.Translators.Models;

public class GeminiResponse
{
    [JsonPropertyName("candidates")] public required List<Candidate> Candidates { get; set; }
}

public class Candidate
{
    [JsonPropertyName("content")] public required ContentItem Content { get; set; }
}