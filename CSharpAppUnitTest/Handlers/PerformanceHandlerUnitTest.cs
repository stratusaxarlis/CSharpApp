using CSharpApp.Infrastructure.Middleware.Handlers;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
namespace UnitTests;

public sealed class PerformanceLoggingHandlerTests
{
    private readonly Mock<ILogger<PerformanceLoggingHandler>> _loggerMock = new();
    private readonly Mock<HttpMessageHandler> _innerHandlerMock = new();
    [Fact]
    public async Task SendAsync_LogsInformation_WhenRequestIsFast()
    {
        // Arrange: Fast response (completes in 0ms)
        _innerHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var handler = new PerformanceLoggingHandler(_loggerMock.Object)
        {
            InnerHandler = _innerHandlerMock.Object
        };

        var client = new HttpClient(handler);

        // Act
        var response = await client.GetAsync("https://api.example.com/test");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify LogInformation was called (and NOT LogWarning)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HTTP Outgoing Request")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task SendAsync_LogsWarning_WhenRequestIsSlow()
    {
        // Arrange: Simulate a slow request (> 1000ms delay)
        _innerHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Returns(async () =>
            {
                await Task.Delay(1050); // Delay over 1 second to trigger slow request threshold
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var handler = new PerformanceLoggingHandler(_loggerMock.Object)
        {
            InnerHandler = _innerHandlerMock.Object
        };

        var client = new HttpClient(handler);

        // Act
        var response = await client.GetAsync("https://api.example.com/test");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify LogWarning was called (and NOT LogInformation)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Slow HTTP Outgoing Request")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}