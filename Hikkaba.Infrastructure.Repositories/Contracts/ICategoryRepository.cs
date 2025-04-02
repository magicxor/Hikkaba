using Hikkaba.Infrastructure.Models.Category;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface ICategoryRepository
{
    Task<IReadOnlyList<CategoryDetailsModel>> ListCategoriesAsync(CategoryFilter categoryFilter, CancellationToken cancellationToken);
    Task<CategoryDetailsModel?> GetCategoryAsync(string categoryAlias, bool includeDeleted, CancellationToken cancellationToken);
    Task<int> CreateCategoryAsync(CategoryCreateRequestModel categoryCreateRequest, CancellationToken cancellationToken);
    Task EditCategoryAsync(CategoryEditRequestModel categoryEditRequest, CancellationToken cancellationToken);
    Task SetCategoryDeletedAsync(int categoryId, bool isDeleted, CancellationToken cancellationToken);
}
