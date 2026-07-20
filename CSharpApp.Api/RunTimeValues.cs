

namespace CSharpApp.Core.Settings;

public static class WebApplicationExtensions
{
    private static bool _isInitialized;
    private static string? _aspNetCoreEnvironment;
    public static string AspNetCoreEnvironment
    {
        get
        {
            _aspNetCoreEnvironment ??= string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                ? Environments.Production
                : Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return _aspNetCoreEnvironment!;
        }
    }


    public static WebApplicationBuilder Initialize(this WebApplicationBuilder webAppBuilder)
    {
        if (!_isInitialized)
        {
            _isInitialized = true;

        }
        else
        {
            throw new InvalidOperationException($"{nameof(WebApplicationExtensions)} is already initialized.");
        }

        return webAppBuilder;
    }
}
