using CSharpApp.Core.Dtos;
using Scalar.AspNetCore;
using Serilog.Sinks.SystemConsole.Themes;

namespace CSharpApp.Api.Extensions;

public static class WebApplicationBuilderExtensions
{
    /// <summary>
    ///     Configures the <see cref="WebApplicationBuilder" /> with default settings, including logging, service defaults, configuration sources,
    ///     Kestrel server settings, Redis integration, and static web assets setup.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder" /> instance to configure.</param>
    /// <returns>The configured <see cref="WebApplicationBuilder" /> instance.</returns>
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
    {

        // Registering Serilog to provide logging functionality throughout the application.
        builder.Host.UseSerilog((context, services, configuration) => configuration
       .ReadFrom.Configuration(context.Configuration)
       .ReadFrom.Services(services)
       .Enrich.FromLogContext()
       .WriteTo.Console(
           theme: AnsiConsoleTheme.Code, // Provides vibrant colors in terminal
           outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}=> {Message:lj}{NewLine}{Exception}"));

        // Define a shutdown timeout of 30 seconds, this is the amount of time the host will wait for the app to stop before it's forcibly terminated.
        builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(30));

        // Configure the Kestrel server to not add a 'Server' header in the responses
        builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

        // Load the main configuration file, which is always required.
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
        builder.Configuration.AddEnvironmentVariables();
        builder.Configuration.AddUserSecrets<Program>();

        // Configure AppSettings
        builder.Services.AddDefaultConfiguration();

        builder.Services.AddOpenApi();
        builder.Services.AddHttpConfiguration();
        builder.Services.AddProblemDetails();
        builder.Services.AddApiVersioning();


        return builder;
    }

    public static WebApplication ConfigureServices(this WebApplication app)
    {

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        app.UseSerilogRequestLogging();
        //app.UseHttpsRedirection();

        var versionedEndpointRouteBuilder = app.NewVersionedApi();
        var productsGroup = versionedEndpointRouteBuilder.MapGroup("api/v{version:apiVersion}/products")
            .HasApiVersion(1.0);


        // GET /api/v1/products
        productsGroup.MapGet("/", async (IProductsService productsService) =>
        {
            var products = await productsService.GetProductsAsync();
            return Results.Ok(products);
        })
        .WithName("GetProducts");

        // GET /api/v1/products/{id}
        productsGroup.MapGet("/{id:int}", async (int id, IProductsService productsService) =>
        {
            var product = await productsService.GetProductByIdAsync(id);
            return product is not null ? Results.Ok(product) : Results.NotFound();
        })
        .WithName("GetProductById");

        // POST /api/v1/products
        productsGroup.MapPost("/", async (CreateProductDto dto, IProductsService productsService) =>
        {
            var created = await productsService.CreateProductAsync(dto);
            return created is not null
                ? Results.Created($"api/v1/products/{created.Id}", created)
                : Results.BadRequest();
        })
        .WithName("CreateProduct");

        // PUT /api/v1/products/{id}
        productsGroup.MapPut("/{id:int}", async (int id, UpdateProductDto dto, IProductsService productsService) =>
        {
            var updated = await productsService.UpdateProductAsync(id, dto);
            return updated is not null ? Results.Ok(updated) : Results.NotFound();
        })
        .WithName("UpdateProduct");

        // DELETE /api/v1/products/{id}
        productsGroup.MapDelete("/{id:int}", async (int id, IProductsService productsService) =>
        {
            var success = await productsService.DeleteProductAsync(id);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProduct");

        return app;
    }

}
