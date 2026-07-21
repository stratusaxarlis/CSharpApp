using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CSharpApp.Application.Categories;

public sealed class CategoriesService(HttpClient httpClient, IOptionsSnapshot<RestApiSettings> restApiSettings, ILogger<CategoriesService> logger, IAuthService authService) : ICategoriesService
{
    private string CategoriesPath => restApiSettings.Value.Categories!;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    public async Task<IReadOnlyCollection<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await httpClient.GetFromJsonAsync<List<Category>>(
            CategoriesPath,
            cancellationToken
        );

        ReadOnlyCollection<Category>? result = categories?.AsReadOnly();

        if (result is null)
        {
            logger.LogWarning("No categories found.");
            return [];
        }

        return result;
    }

    public async Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<Category>($"{CategoriesPath}/{id}", cancellationToken);
    }

    public async Task<Category?> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        string? token = await authService.GetValidAccessTokenAsync(cancellationToken);

        if (token is not null)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        HttpResponseMessage response = await httpClient.PostAsJsonAsync(CategoriesPath, dto,JsonOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // Read the exact error returned by the server
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Create category failed with status {Status}: {Error}", response.StatusCode, errorContent);

            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {errorContent}");
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Category>(cancellationToken: cancellationToken);
    }

    public async Task<Category?> UpdateCategoryAsync(int id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        string? token = await authService.GetValidAccessTokenAsync(cancellationToken);

        if (token is not null)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await httpClient.PutAsJsonAsync($"{CategoriesPath}/{id}", dto, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Category>(cancellationToken: cancellationToken);
    }

    public async Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        string? token = await authService.GetValidAccessTokenAsync(cancellationToken);

        if (token is not null)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        var response = await httpClient.DeleteAsync($"{CategoriesPath}/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
