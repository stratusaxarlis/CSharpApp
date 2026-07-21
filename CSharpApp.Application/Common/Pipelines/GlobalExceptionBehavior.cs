using MediatR;
using Microsoft.AspNetCore.Http;

namespace CSharpApp.Application.Common.Pipelines;

public sealed class GlobalExceptionBehaviour<TRequest, TResponse>(ILogger<TRequest> logger, IHttpContextAccessor httpContextAccessor): IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next().ConfigureAwait(false);
        }
#pragma warning disable S2139
//	Either log this exception and handle it, or rethrow it with some contextual information.
//	In this instance this is acceptable because this middleware is responsible for just logging an exception and
//	allowing the rest of the application flow to deal it
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Request: {RequestName}  failed. Endpoint: {HttpMethod} {RequestPath} TraceId: {TraceId}. Error: {ErrorMessage}. Request Details: {@Request}",
                typeof(TRequest).Name,
                httpContextAccessor.HttpContext?.Request.Method,
                httpContextAccessor.HttpContext?.Request.Path,
                httpContextAccessor.HttpContext?.TraceIdentifier,
                ex.Message,
                request);
            throw;
        }
#pragma warning restore S2139
    }
}
