using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CSharpApp.Infrastructure.Middleware.Handlers;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        string traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        logger.LogError(exception,
            $"Could not process a request on machine {Environment.MachineName}. TraceId: {traceId}");

        await GenerateProblemDetails(httpContext, traceId, exception).ConfigureAwait(false);

        return true;
    }

    private static async Task GenerateProblemDetails(HttpContext httpContext,
        string traceId,
        Exception exception)
    {
        var (statusCode, title) = MapExceptionWithStatusCode(exception);

        await Results.Problem(title: title,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?>
            {
                {
                    "traceId", traceId
                }
            }).ExecuteAsync(httpContext).ConfigureAwait(false);
    }

    private static (int statusCode, string title) MapExceptionWithStatusCode(Exception exception)
    {
        if (exception is not ServerExceptionHandler && exception.InnerException != null)
            while (exception.InnerException != null)
                exception = exception.InnerException;


        return exception switch
        {
            FluentValidation.ValidationException fluentEx => (
                StatusCodes.Status400BadRequest, $"One or more validation failures occurred. {string.Join(", ", fluentEx.Errors.Select(e => e.ErrorMessage))}"),
            ArgumentOutOfRangeException => (StatusCodes.Status400BadRequest, exception.Message),
            ServerExceptionHandler => (StatusCodes.Status500InternalServerError, exception.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "We are sorry for the inconvenience but we are on it.")
        };
    }
}
