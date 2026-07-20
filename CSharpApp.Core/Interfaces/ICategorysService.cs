namespace CSharpApp.Core.Interfaces;

public interface ICategoriesService
{
    Task<IReadOnlyCollection<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Category?> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<Category?> UpdateCategoryAsync(int id, UpdateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
}
