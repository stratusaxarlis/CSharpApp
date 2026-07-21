using System.Reflection;
using CSharpApp.Application.Common;
using CSharpApp.Application.Common.Helpers;
using CSharpApp.Application.Common.Pipelines;
using CSharpApp.Application.Common.Publisher;
using CSharpApp.Infrastructure.Middleware.Handlers;
using FluentValidation;
using MediatR.Pipeline;

namespace CSharpApp.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        Assembly? mediaTrAssembly =typeof(Application.Categories.Queries.GetCategoryByIdQuery).Assembly;
        services.AddValidatorsFromAssembly(mediaTrAssembly);
        services.AddSingleton<IRequestPerformanceState, RequestPerformanceState>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddMediatR(config =>
        {
            config.Lifetime = ServiceLifetime.Scoped;
            config.RegisterServicesFromAssembly(mediaTrAssembly);
            config.NotificationPublisher = new ParallelNoWaitPublisher();
            config.AddRequestPreProcessor(typeof(IRequestPreProcessor<>), typeof(LoggingPreProcessor<>));
            config.AddRequestPreProcessor(typeof(IRequestPreProcessor<>), typeof(ValidationPreProcessor<>));
            config.AddOpenBehavior(typeof(GlobalExceptionBehaviour<,>));
            config.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });
        return services;
    }
}
