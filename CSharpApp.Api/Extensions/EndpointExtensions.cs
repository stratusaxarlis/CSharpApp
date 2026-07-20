using CSharpApp.Core.Settings;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using Asp.Versioning.Builder;

namespace CSharpApp.Api.Extensions;

public static class EndpointExtensions
{
    public static RouteGroupBuilder MapGroup(this WebApplication app, EndpointGroupBase group, string? groupName = null, string? tags = null, string? description = null)
    {
        string finalGroupName = groupName ?? group.GetType().Name;
        IVersionedEndpointRouteBuilder versionedBuilder = app.NewVersionedApi();
        RouteGroupBuilder groupBuilder = versionedBuilder
            .MapGroup($"api/v{{version:apiVersion}}/{finalGroupName}")
            .WithTags(tags ?? finalGroupName)
            .WithDescription(description ?? string.Empty)
            .HasApiVersion(1.0);

        return groupBuilder;
    }
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        Type endpointGroupType = typeof(EndpointGroupBase);
        Assembly assembly = Assembly.GetCallingAssembly();

        IEnumerable<Type> endpointGroupTypes = assembly.GetExportedTypes()
            .Where(t => t.IsSubclassOf(endpointGroupType));

        foreach (Type type in endpointGroupTypes)
        {
            if (Activator.CreateInstance(type) is EndpointGroupBase instance)
            {
                instance.Map(app);
            }
        }

        return app;
    }
}


public static class EndpointRouteBuilderExtensions
{
    private const int RequestTimeoutInMinutes = 20;

    public static IEndpointRouteBuilder MapGet(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "", string? description = null, IEndpointFilter[]? filters = null)
    {


        var endpoint = builder.MapGet(pattern, handler)
            .WithName(handler.Method.Name)
            .WithDescription(description ?? string.Empty)
            .DisableAntiforgery()
            .WithRequestTimeout(TimeSpan.FromMinutes(RequestTimeoutInMinutes));

        RegisterFilters(filters ?? [], endpoint);

        return builder;
    }

    public static IEndpointRouteBuilder MapPost(
        this IEndpointRouteBuilder builder,
        Delegate handler,
        [StringSyntax("Route")] string pattern = "",
        string? description = null,
        IEndpointFilter[]? filters = null,
        string? methodName = null)
    {

        methodName ??= handler.Method.Name;

        var endpoint = builder.MapPost(pattern, handler)
            .WithName(methodName)
            .WithDescription(description ?? string.Empty)
            .DisableAntiforgery()
            .WithRequestTimeout(TimeSpan.FromMinutes(RequestTimeoutInMinutes));

        RegisterFilters(filters ?? [], endpoint);

        return builder;
    }

    public static IEndpointRouteBuilder MapPatch(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "", string? description = null, IEndpointFilter[]? filters = null)
    {


        var endpoint = builder.MapPatch(pattern, handler)
            .WithName(handler.Method.Name)
            .WithDescription(description ?? string.Empty)
            .DisableAntiforgery()
            .WithRequestTimeout(TimeSpan.FromMinutes(RequestTimeoutInMinutes));

        RegisterFilters(filters ?? [], endpoint);

        return builder;
    }

    public static IEndpointRouteBuilder MapPut(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "", string? description = null, IEndpointFilter[]? filters = null)
    {

        var endpoint = builder.MapPut(pattern, handler)
            .WithName(handler.Method.Name)
            .WithDescription(description ?? string.Empty)
            .DisableAntiforgery()
            .WithRequestTimeout(TimeSpan.FromMinutes(RequestTimeoutInMinutes));

        RegisterFilters(filters ?? [], endpoint);

        return builder;
    }

    public static IEndpointRouteBuilder MapDelete(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern, IEndpointFilter[]? filters = null)
    {


        var endpoint = builder.MapDelete(pattern, handler)
            .WithName(handler.Method.Name)
            .DisableAntiforgery()
            .WithRequestTimeout(TimeSpan.FromMinutes(RequestTimeoutInMinutes));

        RegisterFilters(filters ?? [], endpoint);

        return builder;
    }

    private static void RegisterFilters(Span<IEndpointFilter> filters, IEndpointConventionBuilder endpoint)
    {
        foreach (IEndpointFilter endpointFilter in filters)
        {
            endpoint.AddEndpointFilter(endpointFilter);
        }
    }
}
public abstract class EndpointGroupBase
{
    public abstract void Map(WebApplication app);
}
