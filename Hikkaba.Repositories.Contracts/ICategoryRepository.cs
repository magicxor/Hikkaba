using Hikkaba.Infrastructure.Models.Category;

namespace Hikkaba.Repositories.Contracts;

public interface ICategoryRepository
{
    Task<IReadOnlyList<CategoryViewRm>> ListCategoriesAsync(CategoryFilter categoryFilter);
    Task<CategoryViewRm?> GetCategoryAsync(string categoryAlias, bool includeDeleted);
    Task<int> CreateCategoryAsync(CategoryCreateRequestRm categoryCreateRequest);
    Task EditCategoryAsync(CategoryEditRequestRm categoryEditRequest);
    Task SetCategoryDeletedAsync(int categoryId, bool isDeleted);
}
