using CSharpApp.Infrastructure.Middleware.Handlers;
using Microsoft.Extensions.Options;
using Polly;
using System.Diagnostics;
using CSharpApp.Application.Auth;
using CSharpApp.Application.Categories;

namespace CSharpApp.Infrastructure.Configuration;

public static class HttpConfiguration
{
    public static IServiceCollection AddHttpConfiguration(this IServiceCollection services)
    {
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IConfiguration? configuration = serviceProvider.GetService<IConfiguration>();
        services.AddTransient<PerformanceLoggingHandler>();
        services.AddTransient<AuthTokenHandler>();

        HttpClientSettings httpSettings = configuration?.GetSection("HttpClientSettings").Get<HttpClientSettings>() ?? new HttpClientSettings();
        services.AddAuthenticatedTypedHttpClient<IProductsService, ProductsService>(httpSettings);
        services.AddAuthenticatedTypedHttpClient<ICategoriesService, CategoriesService>(httpSettings);
        services.AddTypedHttpClient<IAuthService, AuthService>(httpSettings);

        return services;
    }
    private static IHttpClientBuilder AddAuthenticatedTypedHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        HttpClientSettings httpSettings)
        where TClient : class
        where TImplementation : class, TClient
    {
        return services.AddTypedHttpClient<TClient, TImplementation>(httpSettings)
            .AddHttpMessageHandler<AuthTokenHandler>();
    }
    private static IHttpClientBuilder AddTypedHttpClient<TClient, TImplementation>(this IServiceCollection services, HttpClientSettings httpSettings) where TClient : class where TImplementation : class, TClient
    {
        return services.AddHttpClient<TClient, TImplementation>((sp, client) =>
            {
                RestApiSettings settings = sp.GetRequiredService<IOptions<RestApiSettings>>().Value;

                if (!string.IsNullOrEmpty(settings.BaseUrl))
                {
                    client.BaseAddress = new Uri(settings.BaseUrl);
                }

                client.Timeout = TimeSpan.FromMinutes(Debugger.IsAttached ? 120 : 3);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(httpSettings.LifeTime > 0 ? httpSettings.LifeTime : 2))
            .AddTransientHttpErrorPolicy(policy =>
                policy.WaitAndRetryAsync(
                    httpSettings.RetryCount,
                    _ => TimeSpan.FromSeconds(httpSettings.SleepDuration)
                ))
            .AddHttpMessageHandler<PerformanceLoggingHandler>();
    }
}
