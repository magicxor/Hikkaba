using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Models.Category;

namespace Hikkaba.Application.Contracts;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDetailsModel>> ListCategoriesAsync(CategoryFilter filter, CancellationToken cancellationToken);

    Task<CategoryModeratorsModel> ListCategoryModeratorsAsync(CategoryModeratorsFilter filter, CancellationToken cancellationToken);

    Task<CategoryDetailsModel?> GetCategoryAsync(string alias, bool includeDeleted, CancellationToken cancellationToken);

    Task<int> CreateCategoryAsync(CategoryCreateRequestModel categoryCreateRequest, CancellationToken cancellationToken);

    Task EditCategoryAsync(CategoryEditRequestModel categoryEditRequest, CancellationToken cancellationToken);

    Task SetCategoryDeletedAsync(int id, bool newValue, CancellationToken cancellationToken);
}
