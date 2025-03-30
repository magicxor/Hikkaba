using Hikkaba.Infrastructure.Models.Category;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface ICategoryRepository
{
    Task<IReadOnlyList<CategoryDetailsModel>> ListCategoriesAsync(CategoryFilter categoryFilter);
    Task<CategoryDetailsModel?> GetCategoryAsync(string categoryAlias, bool includeDeleted);
    Task<int> CreateCategoryAsync(CategoryCreateRequestModel categoryCreateRequest);
    Task EditCategoryAsync(CategoryEditRequestModel categoryEditRequest);
    Task SetCategoryDeletedAsync(int categoryId, bool isDeleted);
}
