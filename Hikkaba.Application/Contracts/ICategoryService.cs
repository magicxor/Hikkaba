using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Models.Category;

namespace Hikkaba.Application.Contracts;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDetailsModel>> ListAsync(CategoryFilter filter);

    Task<CategoryModeratorsModel> ListCategoryModeratorsAsync(CategoryModeratorsFilter filter);

    Task<CategoryDetailsModel?> GetAsync(string alias, bool includeDeleted);

    Task<int> CreateAsync(CategoryCreateRequestModel categoryCreateRequest);

    Task EditAsync(CategoryEditRequestModel categoryEditRequest);

    Task SetIsDeletedAsync(int id, bool newValue);
}
