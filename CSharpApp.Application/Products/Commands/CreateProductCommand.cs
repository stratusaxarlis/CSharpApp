using MediatR;

namespace CSharpApp.Application.Products.Commands;

public sealed record CreateProductCommand : IRequest<Result<Product>>
{
    public required CreateProductDto Dto { get; init; } = null!;
}
public sealed class CreateProductCommandHandler(IProductsService productsService, ILogger<CreateProductCommandHandler> logger): IRequestHandler<CreateProductCommand, Result<Product>>
{
    public async Task<Result<Product>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        Product? created = await productsService.CreateProductAsync(request.Dto, cancellationToken);
        if (created is null)
        {
            logger.LogWarning("No products created.");
            return await Result<Product>.FailureAsync("No products created.");
        }
        return await Result<Product>.SuccessAsync(created);
    }
}
