namespace CSharpApp.Core.Dtos;

public record ThirdPartyAuthDto
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
public record JwtTokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; init; }
}
