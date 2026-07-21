using CSharpApp.Infrastructure.Helpers;
using MediatR;

namespace CSharpApp.Application.Products.Queries;

public sealed record GetProductByIdQuery :IRequest<Result<Product>>
{
    public int Id { get; init; }
}
public sealed class GetProductByIdQueryHandler(IProductsService productsService, ILogger<GetProductByIdQueryHandler> logger): IRequestHandler<GetProductByIdQuery, Result<Product>>
{
    public async Task<Result<Product>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        Product? product = await productsService.GetProductByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Product with ID {ProductId} not found.", request.Id);
            return await Result<Product>.FailureAsync($"Product with ID {request.Id} not found.");
        }

        return await Result<Product>.SuccessAsync(product);
    }
}
