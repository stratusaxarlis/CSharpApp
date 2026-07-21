using CSharpApp.Infrastructure.Helpers;
using MediatR;

namespace CSharpApp.Application.Products.Queries;

public sealed record GetProductsQuery : IRequest<Result<IReadOnlyCollection<Product>>>
{
}

public sealed class GetProductsQueryHandler(IProductsService productsService, ILogger<GetProductsQueryHandler> logger): IRequestHandler<GetProductsQuery, Result<IReadOnlyCollection<Product>>>
{
    public async Task<Result<IReadOnlyCollection<Product>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Product> products = await productsService.GetProductsAsync(cancellationToken);

        if (products.Count == 0)
        {
            logger.LogWarning("No products found.");
            return await Result<IReadOnlyCollection<Product>>.FailureAsync("No products found.");
        }
        return await Result<IReadOnlyCollection<Product>>.SuccessAsync(products);
    }
}
