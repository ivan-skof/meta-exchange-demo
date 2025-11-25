using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BSD.Api.ExceptionHandlers;

/// <summary></summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    
    /// <summary></summary>    
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }
    
    /// <summary></summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var exceptionMessage = exception.Message;
        _logger.LogError(
                        exception,
                        "Error occurred: {Message} at {Time}",
                        exception.Message,
                        DateTime.UtcNow);

        var problemDetails = exception switch
        {
            ArgumentException argEx => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Request",
                Detail = argEx.Message,
            },
            FileNotFoundException or DirectoryNotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Configuration Error",
                Detail = "Required data files not found",
            },
            InvalidOperationException invalidOp => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Operation Error",
                Detail = invalidOp.Message,
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred",
            }
        };

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
