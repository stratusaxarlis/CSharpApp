using MediatR;

namespace CSharpApp.Application.Categories.Queries;

public sealed record GetCategoriesQuery : IRequest<IReadOnlyCollection<Category>>;


public sealed class GetCategoriesQueryHandler(ICategoriesService categoriesService, ILogger<GetCategoriesQueryHandler> logger): IRequestHandler<GetCategoriesQuery, IReadOnlyCollection<Category>>
{
    public async Task<IReadOnlyCollection<Category>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Category> categories = await categoriesService.GetCategoriesAsync(cancellationToken);

        if (categories.Count == 0)
        {
            logger.LogWarning("No categories found.");
        }
        return categories;
    }
}
