using CSharpApp.Infrastructure.Helpers;
using MediatR;

namespace CSharpApp.Application.Categories.Queries;


public sealed record GetCategoriesQuery : IRequest<Result<IReadOnlyCollection<Category>>>
{
}


public sealed class GetCategoriesQueryHandler(ICategoriesService categoriesService, ILogger<GetCategoriesQueryHandler> logger): IRequestHandler<GetCategoriesQuery, Result<IReadOnlyCollection<Category>>>
{
    public async Task<Result<IReadOnlyCollection<Category>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Category> categories = await categoriesService.GetCategoriesAsync(cancellationToken);

        if (categories.Count == 0)
        {
            logger.LogWarning("No categories found.");
            return Result<IReadOnlyCollection<Category>>.Failure("No categories found.");
        }
        return Result<IReadOnlyCollection<Category>>.Success(categories);
    }
}
