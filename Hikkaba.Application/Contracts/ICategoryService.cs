using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Models.Category;

namespace Hikkaba.Application.Contracts;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDetailsModel>> ListAsync(CategoryFilter filter, CancellationToken cancellationToken);

    Task<CategoryModeratorsModel> ListCategoryModeratorsAsync(CategoryModeratorsFilter filter, CancellationToken cancellationToken);

    Task<CategoryDetailsModel?> GetAsync(string alias, bool includeDeleted, CancellationToken cancellationToken);

    Task<int> CreateAsync(CategoryCreateRequestModel categoryCreateRequest, CancellationToken cancellationToken);

    Task EditAsync(CategoryEditRequestModel categoryEditRequest, CancellationToken cancellationToken);

    Task SetIsDeletedAsync(int id, bool newValue, CancellationToken cancellationToken);
}
