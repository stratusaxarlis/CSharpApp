using System.Net;
using CSharpApp.Infrastructure.Helpers;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace CSharpApp.Infrastructure.Middleware.Handlers;

public class ServerExceptionHandler(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : Exception(message)
{
    public IEnumerable<string> ErrorMessages { get; } = [message];

    public HttpStatusCode StatusCode { get; } = statusCode;
}

public sealed class ServerExceptionHandler<TRequest, TResponse, TException>(ILogger<ServerExceptionHandler<TRequest, TResponse, TException>> logger) : IRequestExceptionHandler<TRequest, TResponse, TException>
    where TRequest : IRequest<Result>
    where TResponse : Result
    where TException : ServerExceptionHandler
{
    public Task Handle(TRequest request, TException exception, RequestExceptionHandlerState<TResponse> state,
        CancellationToken cancellationToken)
    {
        state.SetHandled((TResponse)Result.Failure(exception.Message));
        logger.LogError(exception, exception.Message);
        return Task.CompletedTask;
    }
}
