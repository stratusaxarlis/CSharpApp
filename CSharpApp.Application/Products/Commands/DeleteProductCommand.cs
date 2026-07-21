using MediatR;

namespace CSharpApp.Application.Products.Commands;

public sealed record DeleteProductCommand : IRequest<Result<bool>>
{
    public int Id { get; init; }
}

public sealed class DeleteProductCommandHandler(IProductsService productsService, ILogger<DeleteProductCommandHandler> logger): IRequestHandler<DeleteProductCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        bool success = await productsService.DeleteProductAsync(request.Id, cancellationToken);
        if (!success)
        {
            logger.LogWarning("Failed to delete product.");
            return await Result<bool>.FailureAsync("Failed to delete product.");
        }
        return await Result<bool>.SuccessAsync(true);
    }
}
