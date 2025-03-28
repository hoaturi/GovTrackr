using FluentResults;
using GovTrackr.Application.Common.Models;
using GovTrackr.Application.Common.Models.Errors;
using Microsoft.AspNetCore.Mvc;

namespace GovTrackr.Application.Common.Controllers;

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

        if (!result.Errors.Any()) return StatusCode(StatusCodes.Status500InternalServerError);

        var error = result.Errors.First();

        var statusCode = GetStatusCodeForError(error);
        var errorResponse = new ErrorResponse(statusCode, error.Message);

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