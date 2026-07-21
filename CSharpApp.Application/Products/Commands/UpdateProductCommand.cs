using MediatR;

namespace CSharpApp.Application.Products.Commands;

public sealed record UpdateProductCommand : IRequest<Result<Product>>
{
    public int Id { get; set; }
    public UpdateProductDto Dto { get; set; } = null!;
}


public sealed class UpdateProductCommandHandler(IProductsService productService, ILogger<UpdateProductCommandHandler> logger): IRequestHandler<UpdateProductCommand, Result<Product>>
{
    public async Task<Result<Product>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        Product? updated = await productService.UpdateProductAsync(request.Id, request.Dto, cancellationToken);
        if (updated is null)
        {
            logger.LogWarning("No products updated.");
            return await Result<Product>.FailureAsync("No products updated.");
        }
        return await Result<Product>.SuccessAsync(updated);
    }
}
