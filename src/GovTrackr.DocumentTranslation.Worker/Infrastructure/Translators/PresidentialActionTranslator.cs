using System.Net.Http.Json;
using System.Text.Json;
using FluentResults;
using GovTrackr.DocumentTranslation.Worker.Application.Dtos;
using GovTrackr.DocumentTranslation.Worker.Application.Errors;
using GovTrackr.DocumentTranslation.Worker.Application.Exceptions;
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
        if (!result.IsSuccess)
            return Result.Fail(result.Errors.First());

        var dto = ParseResponse(result.Value);
        return Result.Ok(dto);
    }

    private static async Task<Result<GeminiResponse>>
        SendTranslationRequestAsync(
            HttpClient httpClient,
            GeminiRequest request,
            CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("", request, cancellationToken);

        // Transient error such as timeout or server error will be retried by Polly
        // Only permanent errors will be handled here
        if (!response.IsSuccessStatusCode)
            return Result.Fail(TranslationErrors.ApiFailed(response.StatusCode));

        var responseContent = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken);
        if (responseContent is null) throw TranslationExceptions.EmptyResponseContent;

        return Result.Ok(responseContent);
    }

    private static TranslatedPresidentialActionDto ParseResponse(GeminiResponse response)
    {
        var content = response.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text;

        // If response is not valid throw exception and rely on MassTransit to retry
        if (string.IsNullOrWhiteSpace(content))
            throw TranslationExceptions.InvalidTextContent;

        var processed = ExtractProcessedOutput(content); // may throw JsonException

        return new TranslatedPresidentialActionDto(
            processed!.Title,
            processed.Summary,
            processed.Details,
            processed.Keywords
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