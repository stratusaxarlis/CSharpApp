using CSharpApp.Api.Extensions;
using CSharpApp.Core.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CSharpApp.Api.Endpoints;

public sealed class CategoryEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        RouteGroupBuilder group = app
            .MapGroup(this, groupName: "categories", tags: "Categories").WithDescription("Endpoints for managing categories");

        // GET /api/v1/categories
        group.MapGet(handler: GetCategoriesAsync, pattern: "", description: "Retrieves all categories");

        // GET /api/v1/categories/{id}
        group.MapGet(handler: GetCategoryByIdAsync, pattern: "{id:int}", description: "Retrieves a category by ID");

        // POST /api/v1/categories
        group.MapPost(handler: CreateCategoryAsync, pattern: "", description: "Creates a new category");

        // PUT /api/v1/categories/{id}
        group.MapPut(handler: UpdateCategoryAsync, pattern: "{id:int}", description: "Updates an existing category by ID");

        // DELETE /api/v1/categories/{id}
        group.MapDelete(handler: DeleteCategoryAsync, pattern: "{id:int}");
    }

    [ProducesResponseType(typeof(IReadOnlyCollection<Category>), StatusCodes.Status200OK)]
    private static async Task<IResult> GetCategoriesAsync(ICategoriesService categoriesService, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Category> categories = await categoriesService.GetCategoriesAsync(cancellationToken);
        return Results.Ok(categories);
    }

    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    private static async Task<IResult> GetCategoryByIdAsync(int id, ICategoriesService categoriesService, CancellationToken cancellationToken = default)
    {
        Category? category = await categoriesService.GetCategoryByIdAsync(id, cancellationToken);
        return category is not null ? Results.Ok(category) : Results.NotFound();
    }

    [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    private static async Task<IResult> CreateCategoryAsync(ICategoriesService categoriesService, CreateCategoryDto dto, HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        Category? created = await categoriesService.CreateCategoryAsync(dto, cancellationToken);
        int version = httpContext.GetRequestedApiVersion()?.MajorVersion ?? 1;

        return created is not null
            ? Results.Created($"api/v{version}/categories/{created.Id}", created)
            : Results.BadRequest();
    }

    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    private static async Task<IResult> UpdateCategoryAsync(int id, UpdateCategoryDto dto, ICategoriesService categoriesService, CancellationToken cancellationToken = default)
    {
        Category? updated = await categoriesService.UpdateCategoryAsync(id, dto, cancellationToken);
        return updated is not null ? Results.Ok(updated) : Results.NotFound();
    }

    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    private static async Task<IResult> DeleteCategoryAsync(int id, ICategoriesService categoriesService, CancellationToken cancellationToken = default)
    {
        bool success = await categoriesService.DeleteCategoryAsync(id, cancellationToken);
        return success ? Results.NoContent() : Results.NotFound();
    }
}
