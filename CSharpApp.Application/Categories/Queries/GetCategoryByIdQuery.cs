using CSharpApp.Infrastructure.Helpers;
using MediatR;

namespace CSharpApp.Application.Categories.Queries;

public sealed record GetCategoryByIdQuery: IRequest<Result<Category>>
{
    public int Id { get; init; }

}
public sealed class GetCategoryByIdQueryHandler(ICategoriesService categoriesService, ILogger<GetCategoryByIdQueryHandler> logger): IRequestHandler<GetCategoryByIdQuery, Result<Category>>
{
    public async Task<Result<Category>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        Category? category = await categoriesService.GetCategoryByIdAsync(request.Id, cancellationToken);

        if (category is null)
        {
            logger.LogWarning("Category with ID {CategoryId} not found.", request.Id);
            return await Result<Category>.FailureAsync($"Category with ID {request.Id} not found.");
        }

        return await Result<Category>.SuccessAsync(category);
    }
}
