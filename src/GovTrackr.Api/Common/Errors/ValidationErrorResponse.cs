using System.Net;

namespace GovTrackr.Api.Common.Errors;

public record ValidationErrorResponse(List<ValidationError> Errors) : ErrorResponse((int)HttpStatusCode.BadRequest,
    "Validation error occurred", ErrorCodes.ValidationError);

public record ValidationError(string Field, string Message);