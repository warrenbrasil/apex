using Apex.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Apex.Api.Controllers;

/// <summary>
/// Base controller with common functionality for API controllers.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Converts a Result to an IActionResult with appropriate status code.
    /// </summary>
    protected IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error.Code switch
        {
            var code when code.EndsWith("NotFound") => NotFound(CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Not Found",
                result.Error.Message)),

            var code when code.EndsWith("AlreadyExists") => Conflict(CreateProblemDetails(
                StatusCodes.Status409Conflict,
                "Conflict",
                result.Error.Message)),

            var code when code.EndsWith("ValidationFailed") => BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Validation Failed",
                result.Error.Message)),

            var code when code.EndsWith("InvalidQuery") => BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Invalid Query",
                result.Error.Message)),

            _ => BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Bad Request",
                result.Error.Message))
        };
    }

    /// <summary>
    /// Converts a Result to a CreatedAtAction result.
    /// </summary>
    protected IActionResult ToCreatedAtActionResult<T>(
        Result<T> result,
        string actionName,
        object routeValues)
    {
        if (result.IsSuccess)
        {
            return CreatedAtAction(actionName, routeValues, result.Value);
        }

        return ToActionResult(result);
    }

    private ProblemDetails CreateProblemDetails(int statusCode, string title, string detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = GetProblemDetailsType(statusCode)
        };
    }

    private static string GetProblemDetailsType(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };
}
