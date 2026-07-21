using CSharpApp.Infrastructure.Helpers;
using MediatR;

namespace CSharpApp.Application.Categories.Commands;

public sealed record CreateCategoryCommand : IRequest<Result<Category>>
{
 public CreateCategoryDto Dto { get; init; } = null!;
}
public sealed class CreateCategoryCommandHandler(ICategoriesService categoriesService, ILogger<CreateCategoryCommandHandler> logger): IRequestHandler<CreateCategoryCommand, Result<Category>>
{
    public async Task<Result<Category>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        Category? created = await categoriesService.CreateCategoryAsync(request.Dto, cancellationToken);
        if (created is null)
        {
            logger.LogWarning("No categories created.");
            return Result<Category>.Failure("No categories created.");
        }
        return Result<Category>.Success(created);
    }
}
