using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Errors;

namespace GovTrackr.Api.Common;

/// <summary>
///     Base API controller that provides a method for converting a Result to an ActionResult.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    ///     Converts a Result to an IActionResult.
    ///     Returns response with status code mapped from error type.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IActionResult containing the result value or an error response.</returns>
    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess) return Ok(result.Value);

        if (result.Errors.Count == 0) return StatusCode(StatusCodes.Status500InternalServerError);

        var error = result.Errors.First();
        error.Metadata.TryGetValue("Code", out var code);

        var statusCode = GetStatusCodeForError(error);
        var errorResponse = new ErrorResponse(statusCode, error.Message, code?.ToString());

        return StatusCode(statusCode, errorResponse);
    }

    private static int GetStatusCodeForError(IError error)
    {
        return error switch
        {
            NotFoundError => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}