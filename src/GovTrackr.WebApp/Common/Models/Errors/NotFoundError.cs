namespace GovTrackr.Application.Common.Models.Errors;

public sealed class NotFoundError(string message = "Resource not found") : ApplicationError(message);