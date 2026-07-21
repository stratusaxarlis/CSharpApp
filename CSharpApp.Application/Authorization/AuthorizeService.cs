
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;

namespace CSharpApp.Application.Auth;

public sealed class AuthService(HttpClient httpClient, IOptionsSnapshot<RestApiSettings> settings, ILogger<AuthService> logger, IMemoryCache cache) : IAuthService
{ private static string TokenCacheKey => "ThirdPartyAuth_JwtToken";
    public async Task<JwtTokenResponse?> LoginAsync(LoginDto dto, CancellationToken  cancellationToken = default)
    {
        try
        {
            RestApiSettings authSettings = settings.Value;
            var credentials = new
            {
                email = dto.Email,
                password = dto.Password
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(authSettings.Auth, credentials, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<JwtTokenResponse>(cancellationToken);
            switch (response.IsSuccessStatusCode)
            {
                case false:
                    logger.LogWarning("Failed to authenticate with 3rd-party service. Status Code: {StatusCode}", response.StatusCode);
                    return null;
                default:
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(50)); // Set buffer before typical 60-min JWT expiry

                    cache.Set(TokenCacheKey, result?.AccessToken, cacheEntryOptions);
                    return result;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while authenticating with the 3rd-party service.");
            return null;
        }
    }
    public async Task<string?> GetValidAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // 1. Check in-memory cache first
        if (cache.TryGetValue(TokenCacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
        {
            return cachedToken;
        }

        // 2. Fall back to automatic login with appsettings credentials
        RestApiSettings credentials = settings.Value;
        LoginDto loginDto = new ()
        {
            Email = credentials.Username,
            Password = credentials.Password
        };

        JwtTokenResponse? authResult = await LoginAsync(loginDto, cancellationToken);
        return authResult?.AccessToken;
    }
}
