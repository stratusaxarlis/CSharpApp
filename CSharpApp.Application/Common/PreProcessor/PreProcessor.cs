using MediatR.Pipeline;

namespace CSharpApp.Application.Common;

public sealed class LoggingPreProcessor<TRequest>(ILogger<TRequest> logger) : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    private readonly ILogger _logger = logger;

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        const string requestName = nameof(TRequest);

        _logger.LogTrace("Processing request of type {RequestName} with details {@Request}", requestName, request);
        return Task.CompletedTask;
    }
}
