namespace GovTrackr.Application.Common.Models;

public record ErrorResponse(int StatusCode, string Message, string? Code);