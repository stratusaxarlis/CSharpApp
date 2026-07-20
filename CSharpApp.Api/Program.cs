using CSharpApp.Core.Dtos;
using Scalar.AspNetCore;
using Serilog.Sinks.SystemConsole.Themes;
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        theme: AnsiConsoleTheme.Code, // Provides vibrant colors in terminal
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}=> {Message:lj}{NewLine}{Exception}"
    )
);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDefaultConfiguration();
builder.Services.AddHttpConfiguration();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning();

var app = builder.Build();

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

app.Run();