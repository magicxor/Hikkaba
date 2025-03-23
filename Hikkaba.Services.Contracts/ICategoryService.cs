using System.Collections.Generic;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Models.Category;

namespace Hikkaba.Services.Contracts;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> ListAsync(CategoryFilter filter);

    Task<CategoryModeratorsRm> ListCategoryModeratorsAsync(CategoryModeratorsFilter filter);

    Task<CategoryDto> GetAsync(int id);

    Task<CategoryDto?> GetAsync(string alias, bool includeDeleted);

    Task<int> CreateAsync(CategoryCreateRequestRm categoryCreateRequest);

    Task EditAsync(CategoryEditRequestRm categoryEditRequest);

    Task SetIsDeletedAsync(int id, bool newValue);
}
