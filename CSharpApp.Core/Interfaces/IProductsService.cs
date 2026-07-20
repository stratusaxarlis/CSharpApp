namespace CSharpApp.Core.Interfaces;

public interface IProductsService
{
    Task<IReadOnlyCollection<Product>> GetProductsAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Product?> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<Product?> UpdateProductAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
}