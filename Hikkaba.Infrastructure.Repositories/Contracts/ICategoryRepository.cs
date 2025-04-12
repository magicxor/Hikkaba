using Hikkaba.Infrastructure.Models.Category;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface ICategoryRepository
{
    Task<IReadOnlyList<CategoryDetailsModel>> ListCategoriesAsync(CategoryFilter filter, CancellationToken cancellationToken);
    Task<CategoryDetailsModel?> GetCategoryAsync(string categoryAlias, bool includeDeleted, CancellationToken cancellationToken);
    Task<int> CreateCategoryAsync(CategoryCreateRequestModel requestModel, CancellationToken cancellationToken);
    Task EditCategoryAsync(CategoryEditRequestModel requestModel, CancellationToken cancellationToken);
    Task SetCategoryModeratorsAsync(string alias, IReadOnlyList<int> moderators, CancellationToken cancellationToken);
    Task SetCategoryDeletedAsync(string alias, bool isDeleted, CancellationToken cancellationToken);
}
