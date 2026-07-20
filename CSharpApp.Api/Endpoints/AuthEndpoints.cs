using CSharpApp.Api.Extensions;
using CSharpApp.Core.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CSharpApp.Api.Endpoints;

public sealed class AuthEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        RouteGroupBuilder group = app
            .MapGroup(this, groupName: "auth", tags: "Auth")
            .WithDescription("Endpoints for authentication operations");

        // POST /api/v1/auth/login
        group.MapPost(handler: LoginAsync, pattern: "login", description: "Authenticates a user and returns access and refresh JWT tokens");
    }

    [ProducesResponseType(typeof(JwtTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    private static async Task<IResult> LoginAsync(LoginDto dto, IAuthService authService, CancellationToken cancellationToken = default)
    {
        JwtTokenResponse? response = await authService.LoginAsync(dto, cancellationToken);

        return response is not null
            ? Results.Ok(response)
            : Results.Unauthorized();
    }
}
