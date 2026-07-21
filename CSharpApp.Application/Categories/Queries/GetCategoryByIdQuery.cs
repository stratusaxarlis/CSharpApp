using CSharpApp.Infrastructure.Helpers;
using MediatR;

namespace CSharpApp.Application.Categories.Queries;

public sealed record GetCategoryByIdQuery: IRequest<Result<Category>>
{
    public int Id { get; set; }

}
public sealed class GetCategoryByIdQueryQueryHandler(ICategoriesService categoriesService, ILogger<GetCategoryByIdQueryQueryHandler> logger): IRequestHandler<GetCategoryByIdQuery, Result<Category>>
{
    public async Task<Result<Category>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        Category? category = await categoriesService.GetCategoryByIdAsync(request.Id, cancellationToken);

        if (category is null)
        {
            logger.LogWarning("Category with ID {CategoryId} not found.", request.Id);
            return Result<Category>.Failure($"Category with ID {request.Id} not found.");
        }

        return Result<Category>.Success(category);
    }
}
