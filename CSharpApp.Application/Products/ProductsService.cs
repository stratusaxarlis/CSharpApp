using System.Net.Http.Json;

namespace CSharpApp.Application.Products;

public sealed class ProductsService(HttpClient httpClient, IOptions<RestApiSettings> restApiSettings, ILogger<ProductsService> logger) : IProductsService
{
    private string ProductsPath => restApiSettings.Value.Products!;
    public async Task<IReadOnlyCollection<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
       
        var products = await httpClient.GetFromJsonAsync<List<Product>>(
            ProductsPath
        );

        var result = products?.AsReadOnly();

        if (result is null)
        {
            logger.LogWarning("No products found.");
            return [];
        }

        return result;
    }
    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<Product>($"{ProductsPath}/{id}", cancellationToken);
    }

    public async Task<Product?> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(ProductsPath, dto, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Product>(cancellationToken: cancellationToken);
    }

    public async Task<Product?> UpdateProductAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"{ProductsPath}/{id}", dto, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Product>(cancellationToken: cancellationToken);
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"{ProductsPath}/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}