using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CSharpApp.Application.Products;

public sealed class ProductsService(HttpClient httpClient, IOptionsSnapshot<RestApiSettings> restApiSettings, ILogger<ProductsService> logger) : IProductsService
{
    private string ProductsPath => restApiSettings.Value.Products!;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<IReadOnlyCollection<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        List<Product>? products = await httpClient.GetFromJsonAsync<List<Product>>(
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


        HttpResponseMessage response = await httpClient.PostAsJsonAsync(ProductsPath, dto,JsonOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // Read the exact error returned by the server
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Create product failed with status {Status}: {Error}", response.StatusCode, errorContent);

            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {errorContent}");
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Product>(cancellationToken: cancellationToken);
    }

    public async Task<Product?> UpdateProductAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {

        HttpResponseMessage response = await httpClient.PutAsJsonAsync(ProductsPath, dto,JsonOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // Read the exact error returned by the server
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Update product failed with status {Status}: {Error}", response.StatusCode, errorContent);

            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {errorContent}");
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Product>(cancellationToken: cancellationToken);
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {

        HttpResponseMessage response = await httpClient.DeleteAsync($"{ProductsPath}/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
