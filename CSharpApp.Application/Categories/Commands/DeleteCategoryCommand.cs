using CSharpApp.Infrastructure.Helpers;
using MediatR;

namespace CSharpApp.Application.Categories.Commands;

public sealed record DeleteCategoryCommand:IRequest<Result<bool>>
{
    public int Id { get; init; }
}

public sealed class DeleteCategoryCommandHandler(ICategoriesService categoriesService, ILogger<CreateCategoryCommandHandler> logger): IRequestHandler<DeleteCategoryCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        bool success = await categoriesService.DeleteCategoryAsync(request.Id, cancellationToken);
        if (!success)
        {
            logger.LogWarning("Failed to delete category.");
            return await Result<bool>.FailureAsync("Failed to delete category.");
        }
        return await Result<bool>.SuccessAsync(true);
    }
}
