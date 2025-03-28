using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Repositories.Contracts;
using Hikkaba.Services.Contracts;

namespace Hikkaba.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryViewRm>> ListAsync(CategoryFilter filter)
    {
        return await _categoryRepository.ListCategoriesAsync(filter);
    }

    public async Task<CategoryModeratorsRm> ListCategoryModeratorsAsync(CategoryModeratorsFilter filter)
    {
        //return await _categoryRepository.ListCategoryModeratorsAsync(filter);
        throw new NotImplementedException();
    }

    public async Task<CategoryViewRm?> GetAsync(string alias, bool includeDeleted)
    {
        return await _categoryRepository.GetCategoryAsync(alias, includeDeleted);
    }

    public async Task<int> CreateAsync(CategoryCreateRequestRm categoryCreateRequest)
    {
        return await _categoryRepository.CreateCategoryAsync(categoryCreateRequest);
    }

    public async Task EditAsync(CategoryEditRequestRm categoryEditRequest)
    {
        await _categoryRepository.EditCategoryAsync(categoryEditRequest);
    }

    public async Task SetIsDeletedAsync(int id, bool newValue)
    {
        await _categoryRepository.SetCategoryDeletedAsync(id, newValue);
    }
}
