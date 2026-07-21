using CSharpApp.Api.Extensions;
using CSharpApp.Application.Products.Queries;
using CSharpApp.Core.Dtos;
using CSharpApp.Infrastructure.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CSharpApp.Api.Endpoints;

public sealed class ProductEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup(this, groupName: "products", tags: "Products").WithDescription("Endpoints for managing products");

        // GET /api/v1/products
        group.MapGet(handler: GetProductsAsync, pattern: "", description: "Retrieves all products");
        // GET /api/v1/products/{id}
        group.MapGet(handler: GetProductByIdAsync, pattern: "{id:int}", description: "Retrieves a product by ID");
        // POST /api/v1/products
        group.MapPost(handler: CreateProductAsync, pattern: "", description: "Creates a product");
        // PUT /api/v1/products/{id}
        group.MapPut(handler: UpdateProductAsync, pattern: "{id:int}", description: "Updates an existing product by ID");
        // DELETE /api/v1/products/{id}
        group.MapDelete(handler: DeleteProductAsync, pattern: "{id:int}");
    }

    [ProducesResponseType(typeof(Result<IReadOnlyCollection<Product>>), StatusCodes.Status200OK)]
    private static async Task<IResult> GetProductsAsync([FromServices] ISender sender, CancellationToken cancellationToken = default)
    {
        Result<IReadOnlyCollection<Product>> products = await sender.Send(new GetProductsQuery(), cancellationToken);
        return Results.Ok(products);
    }

    [ProducesResponseType(typeof(IReadOnlyCollection<Product>), StatusCodes.Status200OK)]
    private static async Task<IResult> CreateProductAsync(IProductsService productsService, CreateProductDto dto, HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        Product? created = await productsService.CreateProductAsync(dto, cancellationToken);

        int version = httpContext.GetRequestedApiVersion()?.MajorVersion ?? 1;

        return created is not null
            ? Results.Created($"api/v{version}/products/{created.Id}", created)
            : Results.BadRequest();
    }
    [ProducesResponseType(typeof(Result<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    private static async Task<IResult> GetProductByIdAsync(int id, [FromServices] ISender sender, CancellationToken cancellationToken = default)
    {
        GetProductByIdQuery query = new GetProductByIdQuery { Id = id };
        Result<Product> result = await sender.Send(query, cancellationToken);
        return result.Data is not null ? Results.Ok(result.Data) : Results.NotFound();
    }

    [ProducesResponseType(typeof(Result<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    private static async Task<IResult> UpdateProductAsync(int id, UpdateProductDto dto, IProductsService productsService, CancellationToken cancellationToken = default)
    {
        Product? updated = await productsService.UpdateProductAsync(id, dto, cancellationToken);
        return updated is not null ? Results.Ok(updated) : Results.NotFound();
    }

    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    private static async Task<IResult> DeleteProductAsync(int id, IProductsService productsService, CancellationToken cancellationToken = default)
    {
        bool success = await productsService.DeleteProductAsync(id, cancellationToken);
        return success ? Results.NoContent() : Results.NotFound();
    }
}
