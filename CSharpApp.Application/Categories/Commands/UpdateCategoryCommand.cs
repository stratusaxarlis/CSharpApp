using CSharpApp.Infrastructure.Helpers;
using MediatR;

namespace CSharpApp.Application.Categories.Commands;

public sealed record UpdateCategoryCommand  :IRequest<Result<Category>>
{
    public int Id { get; init; }
    public UpdateCategoryDto Dto { get; init; } = null!;
}


public sealed class UpdateCategoryCommandHandler(ICategoriesService categoriesService, ILogger<CreateCategoryCommandHandler> logger): IRequestHandler<UpdateCategoryCommand, Result<Category>>
{
    public async Task<Result<Category>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        Category? updated = await categoriesService.UpdateCategoryAsync(request.Id, request.Dto, cancellationToken);
        if (updated is null)
        {
            logger.LogWarning("No categories updated.");
            return Result<Category>.Failure("No categories updated.");
        }
        return Result<Category>.Success(updated);
    }
}
