
namespace CSharpAppUnitTest.Auth;
using CSharpApp.Core.Interfaces;

using CSharpApp.Core.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;



public sealed class AuthEndpointsTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();

    // --------------------------------------------------------------------------------
    // LoginAsync Tests
    // --------------------------------------------------------------------------------

    [Fact]
    public async Task LoginAsync_ReturnsOkWithAuthResponseDto_WhenCredentialsAreValid()
    {
        // Arrange
        LoginDto loginDto = new LoginDto()
        {
            Email = "john@mail.com",
            Password = "changeme"
        };
        JwtTokenResponse expectedResponse = new JwtTokenResponse
        {
            AccessToken = "valid-access-token",
            RefreshToken = "valid-refresh-token"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(loginDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await InvokeLoginEndpoint(loginDto);

        // Assert
        var okResult = Assert.IsType<Ok<JwtTokenResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("valid-access-token", okResult.Value.AccessToken);
        Assert.Equal("valid-refresh-token", okResult.Value.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_ReturnsUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        LoginDto loginDto = new LoginDto()
        {
            Email = "wrong@mail.com",
            Password = "badpassword"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(loginDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JwtTokenResponse?)null);

        // Act
        var result = await InvokeLoginEndpoint(loginDto);

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    // --------------------------------------------------------------------------------
    // Helper Method to Invoke Private Endpoint Handler via Reflection
    // --------------------------------------------------------------------------------

    private async Task<IResult> InvokeLoginEndpoint(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var methodInfo = typeof(CSharpApp.Api.Endpoints.AuthEndpoints)
            .GetMethod("LoginAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.NotNull(methodInfo);

        var task = (Task<IResult>)methodInfo.Invoke(null, [dto, _authServiceMock.Object, cancellationToken])!;
        return await task;
    }
}
