using System.ComponentModel.DataAnnotations;

namespace CSharpApp.Core.Dtos;

public record LoginDto
{
    [EmailAddress]
    public required string Email { get; init; }
    [DataType(DataType.Password)]
    public required string Password { get; init; }
}
public record JwtTokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; init; }
}
