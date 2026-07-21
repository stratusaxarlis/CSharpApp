using System.Net.Http.Headers;

namespace CSharpApp.Infrastructure.Middleware.Handlers;

public sealed class AuthTokenHandler(IAuthService authService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Automatically fetch valid token from cache/service
        string? token = await authService.GetValidAccessTokenAsync(cancellationToken);

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
