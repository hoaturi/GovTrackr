namespace GovTrackr.Api.Common.Errors;

public record ErrorResponse(int StatusCode, string Message, string? Code);