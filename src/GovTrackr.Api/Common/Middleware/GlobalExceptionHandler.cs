using System.Net;
using FluentValidation;
using GovTrackr.Api.Common.Errors;
using Microsoft.AspNetCore.Diagnostics;

namespace GovTrackr.Api.Common.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            var errorList = validationException.Errors
                .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
                .ToList();

            var validationErrorResponse = new ValidationErrorResponse(errorList);

            httpContext.Response.StatusCode = validationErrorResponse.StatusCode;
            await httpContext.Response.WriteAsJsonAsync(validationErrorResponse, cancellationToken);
            return true;
        }

        var errorResponse = new ErrorResponse(
            (int)HttpStatusCode.InternalServerError,
            "An unexpected error occurred",
            ErrorCodes.InternalServerError
        );

        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
        return true;
    }
}