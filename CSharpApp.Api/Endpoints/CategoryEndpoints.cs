using CSharpApp.Api.Extensions;
using CSharpApp.Application.Categories.Commands;
using CSharpApp.Application.Categories.Queries;
using CSharpApp.Core.Dtos;
using CSharpApp.Infrastructure.Helpers;
using MediatR;
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

    [ProducesResponseType(typeof( Result<IReadOnlyCollection<Category>>), StatusCodes.Status200OK)]
    private static async Task<IResult> GetCategoriesAsync([FromServices] ISender sender, CancellationToken cancellationToken = default)
    {
        Result<IReadOnlyCollection<Category>> categories = await sender.Send(new GetCategoriesQuery(), cancellationToken);
        return Results.Ok(categories);
    }

    [ProducesResponseType(typeof( Result<Category>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    private static async Task<IResult> GetCategoryByIdAsync([FromServices] ISender sender,int id, CancellationToken cancellationToken = default)
    {   GetCategoryByIdQuery query = new GetCategoryByIdQuery { Id = id };
        Result<Category>? category = await sender.Send(query, cancellationToken);
        return category is not null ? Results.Ok(category) : Results.NotFound();
    }

    [ProducesResponseType(typeof( Result<Category>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    private static async Task<IResult> CreateCategoryAsync([FromServices] ISender sender, CreateCategoryDto dto, HttpContext httpContext, CancellationToken cancellationToken = default)
    {

        Result<Category>? created = await sender.Send(new CreateCategoryCommand { Dto = dto }, cancellationToken);
        int version = httpContext.GetRequestedApiVersion()?.MajorVersion ?? 1;

        return created is not null && created.Data is not null && created.Data.Id > 0
            ? Results.Created($"api/v{version}/categories/{created?.Data?.Id}", created)
            : Results.BadRequest();
    }

    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    private static async Task<IResult> UpdateCategoryAsync([FromServices] ISender sender, int id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        Result<Category>? updated = await sender.Send(new UpdateCategoryCommand { Id = id, Dto = dto }, cancellationToken);
        return updated is not null ? Results.Ok(updated) : Results.NotFound();
    }

    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    private static async Task<IResult> DeleteCategoryAsync(int id, [FromServices] ISender sender, CancellationToken cancellationToken = default)
    {
        Result<bool> success = await sender.Send(new DeleteCategoryCommand { Id = id }, cancellationToken);
        return success.Data ? Results.NoContent() : Results.NotFound();
    }
}
