using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Repositories.Contracts;

namespace Hikkaba.Application.Implementations;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryDetailsModel>> ListCategoriesAsync(CategoryFilter filter, CancellationToken cancellationToken)
    {
        return await _categoryRepository.ListCategoriesAsync(filter, cancellationToken);
    }

    public async Task<CategoryModeratorsModel> ListCategoryModeratorsAsync(CategoryModeratorsFilter filter, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<CategoryDetailsModel?> GetCategoryAsync(string alias, bool includeDeleted, CancellationToken cancellationToken)
    {
        return await _categoryRepository.GetCategoryAsync(alias, includeDeleted, cancellationToken);
    }

    public async Task<int> CreateCategoryAsync(CategoryCreateRequestModel categoryCreateRequest, CancellationToken cancellationToken)
    {
        return await _categoryRepository.CreateCategoryAsync(categoryCreateRequest, cancellationToken);
    }

    public async Task EditCategoryAsync(CategoryEditRequestModel categoryEditRequest, CancellationToken cancellationToken)
    {
        await _categoryRepository.EditCategoryAsync(categoryEditRequest, cancellationToken);
    }

    public async Task SetCategoryDeletedAsync(int id, bool newValue, CancellationToken cancellationToken)
    {
        await _categoryRepository.SetCategoryDeletedAsync(id, newValue, cancellationToken);
    }
}
