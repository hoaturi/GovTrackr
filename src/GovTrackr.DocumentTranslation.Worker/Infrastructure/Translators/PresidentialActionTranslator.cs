using System.Net.Http.Json;
using System.Text.Json;
using FluentResults;
using GovTrackr.DocumentTranslation.Worker.Application;
using GovTrackr.DocumentTranslation.Worker.Application.Interfaces;
using GovTrackr.DocumentTranslation.Worker.Infrastructure.Translators.Models;
using Shared.Domain.Common;

namespace GovTrackr.DocumentTranslation.Worker.Infrastructure.Translators;

public class PresidentialActionTranslator(
    IHttpClientFactory httpClientFactory,
    IPromptProvider promptProvider)
    : ITranslator
{
    private const string GeminiClientName = "GeminiClient";

    public async Task<Result<TranslatedPresidentialActionDto>> TranslateAsync(
        string title,
        string content,
        CancellationToken cancellationToken)
    {
        using var httpClient = httpClientFactory.CreateClient(GeminiClientName);
        var request = BuildGeminiRequest(title, content);

        var result = await SendTranslationRequestAsync(httpClient, request, cancellationToken);
        if (!result.Success)
            return Result.Fail(new TranslationError(result.ErrorMessage));

        var dto = ParseResponse(result.Response!);
        return Result.Ok(dto);
    }

    private static async Task<(bool Success, GeminiResponse? Response, string ErrorMessage)>
        SendTranslationRequestAsync(
            HttpClient httpClient,
            GeminiRequest request,
            CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return (false, null, "Translation API responded with a failure status code.");

        var responseContent = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken);

        return responseContent is null
            ? (false, null, "Failed to deserialize translation response.")
            : (true, responseContent, string.Empty);
    }

    private static TranslatedPresidentialActionDto ParseResponse(GeminiResponse response)
    {
        var content = response.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text;

        // If response is not valid throw exception and rely on MassTransit to retry
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Response does not contain valid text content.");

        var processed = ExtractProcessedOutput(content); // may throw JsonException

        return new TranslatedPresidentialActionDto(
            processed!.Title,
            processed.Summary,
            processed.Details
        );
    }

    private static ProcessedOutput? ExtractProcessedOutput(string text)
    {
        // This is workaround.
        // Currently Gemini API returns invalid JSON with extra text before and after the JSON object.
        var jsonStart = text.IndexOf('{');
        var jsonEnd = text.LastIndexOf('}');

        if (jsonStart < 0 || jsonEnd <= jsonStart)
            return null;

        var json = text[jsonStart..(jsonEnd + 1)];

        return JsonSerializer.Deserialize<ProcessedOutput>(json);
    }

    private GeminiRequest BuildGeminiRequest(string title, string content)
    {
        var prompt = promptProvider.GetPrompt(DocumentCategoryType.PresidentialAction);

        return new GeminiRequest
        {
            Contents =
            [
                new ContentItem
                {
                    Parts =
                    [
                        new Part { Text = prompt },
                        new Part { Text = $"DOCUMENT_INPUT_TITLE:\n\n{title}" },
                        new Part { Text = $"DOCUMENT_INPUT_CONTENT:\n\n{content}" }
                    ]
                }
            ],
            GenerationConfig = new GenerationConfig
            {
                Temperature = 0.3
            }
        };
    }
}