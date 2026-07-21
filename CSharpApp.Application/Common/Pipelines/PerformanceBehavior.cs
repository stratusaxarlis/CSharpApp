using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CSharpApp.Application.Common.Pipelines;

public sealed class PerformanceBehaviour<TRequest, TResponse>(IRequestPerformanceState performanceState, ILogger<PerformanceBehaviour<TRequest, TResponse>> logger,
    IHttpContextAccessor contextAccessor): IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    ///     Logs warnings if a request takes longer to execute than a specified threshold.
    /// </summary>
    /// <param name="request">The incoming request</param>
    /// <param name="next">The delegate for the next action in the pipeline process.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response from the next delegate</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Stopwatch? timer = null;

        int count = performanceState.Increment();
        if (count > 3)
            timer = Stopwatch.StartNew();

        TResponse response = await next().ConfigureAwait(false);

        timer?.Stop();
        long? elapsedMilliseconds = timer?.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            string requestName = typeof(TRequest).Name;

            logger.LogWarning(
                "Long-running request detected: {RequestName} ({ElapsedMilliseconds}ms) {@Request}",
                requestName,
                elapsedMilliseconds,
                request);
        }

        return response;
    }
}
