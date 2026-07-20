using CSharpApp.Infrastructure.Middleware.Handlers;
using Microsoft.Extensions.Options;
using Polly;
using System.Diagnostics;

namespace CSharpApp.Infrastructure.Configuration;

public static class HttpConfiguration
{
    public static IServiceCollection AddHttpConfiguration(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetService<IConfiguration>();
        services.AddTransient<PerformanceLoggingHandler>();
        HttpClientSettings httpSettings = configuration?.GetSection("HttpClientSettings").Get<HttpClientSettings>() ?? new HttpClientSettings();

        services.AddHttpClient<IProductsService, ProductsService>((serviceProvider, client) =>
        {
            // Retrieve your settings to set BaseAddress automatically
            var settings = serviceProvider.GetRequiredService<IOptions<RestApiSettings>>().Value;

            if (!string.IsNullOrEmpty(settings.BaseUrl))
            {
                client.BaseAddress = new Uri(settings.BaseUrl);
            }

            // Set your timeout logic
            client.Timeout = TimeSpan.FromMinutes(Debugger.IsAttached ? 120 : 3);
        }).SetHandlerLifetime(TimeSpan.FromMinutes(httpSettings.LifeTime > 0 ? httpSettings.LifeTime : 2))
       .AddTransientHttpErrorPolicy(policy =>
            policy.WaitAndRetryAsync(httpSettings.RetryCount, _ => TimeSpan.FromSeconds(httpSettings.SleepDuration))).AddHttpMessageHandler<PerformanceLoggingHandler>();

        return services;
    }
}