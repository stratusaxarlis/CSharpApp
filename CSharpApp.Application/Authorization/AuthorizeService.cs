
using System.Net.Http.Json;
namespace CSharpApp.Application.Auth;

public sealed class AuthService(HttpClient httpClient, IOptionsSnapshot<RestApiSettings> settings, ILogger<AuthService> logger) : IAuthService
{
    public async Task<JwtTokenResponse?> LoginAsync(ThirdPartyAuthDto dto)
    {
        try
        {
            RestApiSettings authSettings = settings.Value;
            var credentials = new
            {
                email = dto.Email,
                password = dto.Password
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(authSettings.Auth, credentials);

            switch (response.IsSuccessStatusCode)
            {
                case false:
                    logger.LogWarning("Failed to authenticate with 3rd-party service. Status Code: {StatusCode}", response.StatusCode);
                    return null;
                default:
                    return await response.Content.ReadFromJsonAsync<JwtTokenResponse>();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while authenticating with the 3rd-party service.");
            return null;
        }
    }
}
