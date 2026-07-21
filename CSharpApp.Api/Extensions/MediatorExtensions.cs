using System.Reflection;
using CSharpApp.Application.Common;
using CSharpApp.Application.Common.Pipelines;
using CSharpApp.Application.Common.Publisher;
using FluentValidation;
using MediatR.Pipeline;

namespace CSharpApp.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        Assembly? mediaTrAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(x => x.FullName!.Contains("CSharp.Application"));
        services.AddValidatorsFromAssembly(mediaTrAssembly ?? Assembly.GetCallingAssembly());


        services.AddMediatR(config =>
        {
            config.Lifetime = ServiceLifetime.Scoped;
            config.RegisterServicesFromAssembly(mediaTrAssembly ?? Assembly.GetCallingAssembly());
            config.NotificationPublisher = new ParallelNoWaitPublisher();
            config.AddRequestPreProcessor(typeof(IRequestPreProcessor<>), typeof(LoggingPreProcessor<>))
                .AddOpenBehavior(typeof(GlobalExceptionBehaviour<,>));
            config.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });
        return services;
    }
}
