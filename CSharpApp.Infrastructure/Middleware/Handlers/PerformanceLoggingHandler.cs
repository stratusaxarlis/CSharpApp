using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CSharpApp.Infrastructure.Middleware.Handlers;

public sealed class PerformanceLoggingHandler(ILogger<PerformanceLoggingHandler> logger) : DelegatingHandler
{
    private const long SlowRequestThresholdMs = 1000; // 1 second

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestUri = request.RequestUri?.ToString() ?? "Unknown URI";
        var method = request.Method.Method;

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            var statusCode = (int)response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (elapsedMs > SlowRequestThresholdMs)
            {
                logger.LogWarning(
                    "Slow HTTP Outgoing Request [{Method}] {Uri} completed in {ElapsedMs}ms with Status {StatusCode}",
                    method, requestUri, elapsedMs, statusCode);
            }
            else
            {
                logger.LogInformation(
                    "HTTP Outgoing Request [{Method}] {Uri} completed in {ElapsedMs}ms with Status {StatusCode}",
                    method, requestUri, elapsedMs, statusCode);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "HTTP Outgoing Request [{Method}] {Uri} failed after {ElapsedMs}ms",
                method, requestUri, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}