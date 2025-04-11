namespace GovTrackr.Api.Common;

public record ErrorResponse(int StatusCode, string Message, string? Code);