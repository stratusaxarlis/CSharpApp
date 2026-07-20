using CSharpApp.Api.Extensions;
using CSharpApp.Core.Dtos;
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

    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    private static async Task<IResult> GetProductsAsync(IProductsService productsService, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Product> products = await productsService.GetProductsAsync(cancellationToken);
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
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    private static async Task<IResult> GetProductByIdAsync(int id, IProductsService productsService, CancellationToken cancellationToken = default)
    {
        Product? product = await productsService.GetProductByIdAsync(id, cancellationToken);
        return product is not null ? Results.Ok(product) : Results.NotFound();
    }

    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
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
