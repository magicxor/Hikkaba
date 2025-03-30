using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Repositories.Contracts;

namespace Hikkaba.Application.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryDetailsModel>> ListAsync(CategoryFilter filter)
    {
        return await _categoryRepository.ListCategoriesAsync(filter);
    }

    public async Task<CategoryModeratorsModel> ListCategoryModeratorsAsync(CategoryModeratorsFilter filter)
    {
        //return await _categoryRepository.ListCategoryModeratorsAsync(filter);
        throw new NotImplementedException();
    }

    public async Task<CategoryDetailsModel?> GetAsync(string alias, bool includeDeleted)
    {
        return await _categoryRepository.GetCategoryAsync(alias, includeDeleted);
    }

    public async Task<int> CreateAsync(CategoryCreateRequestModel categoryCreateRequest)
    {
        return await _categoryRepository.CreateCategoryAsync(categoryCreateRequest);
    }

    public async Task EditAsync(CategoryEditRequestModel categoryEditRequest)
    {
        await _categoryRepository.EditCategoryAsync(categoryEditRequest);
    }

    public async Task SetIsDeletedAsync(int id, bool newValue)
    {
        await _categoryRepository.SetCategoryDeletedAsync(id, newValue);
    }
}
