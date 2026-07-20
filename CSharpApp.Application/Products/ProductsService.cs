using System.Net.Http.Json;

namespace CSharpApp.Application.Products;

public sealed class ProductsService(HttpClient httpClient, IOptions<RestApiSettings> restApiSettings, ILogger<ProductsService> logger) : IProductsService
{
    public async Task<IReadOnlyCollection<Product>> GetProducts()
    {
       
        var products = await httpClient.GetFromJsonAsync<List<Product>>(
            restApiSettings.Value.Products
        );

        var result = products?.AsReadOnly();

        if (result is null)
        {
            logger.LogWarning("No products found.");
            return [];
        }

        return result;
    }
}