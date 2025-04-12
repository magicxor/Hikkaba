using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Models.Category;

namespace Hikkaba.Application.Contracts;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDetailsModel>> ListCategoriesAsync(CategoryFilter filter, CancellationToken cancellationToken);

    Task<CategoryModeratorsModel> ListCategoryModeratorsAsync(CategoryModeratorsFilter filter, CancellationToken cancellationToken);

    Task<CategoryDetailsModel?> GetCategoryAsync(string alias, bool includeDeleted, CancellationToken cancellationToken);

    Task<int> CreateCategoryAsync(CategoryCreateRequestModel requestModel, CancellationToken cancellationToken);

    Task EditCategoryAsync(CategoryEditRequestModel requestModel, CancellationToken cancellationToken);

    Task SetCategoryModeratorsAsync(string alias, IReadOnlyList<int> moderators, CancellationToken cancellationToken);

    Task SetCategoryDeletedAsync(string alias, bool newValue, CancellationToken cancellationToken);
}
