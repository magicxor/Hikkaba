using Hikkaba.Common.Services.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Paging.Extensions;
using Hikkaba.Repositories.Contracts;
using Hikkaba.Repositories.Implementations.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Repositories.Implementations;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IUserContext _userContext;
    private readonly TimeProvider _timeProvider;

    public CategoryRepository(
        ApplicationDbContext applicationDbContext,
        IUserContext userContext,
        TimeProvider timeProvider)
    {
        _applicationDbContext = applicationDbContext;
        _userContext = userContext;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<CategoryViewRm>> ListCategoriesAsync(CategoryFilter categoryFilter)
    {
        var query = _applicationDbContext.Categories
            .Include(category => category.CreatedBy)
            .Include(category => category.ModifiedBy)
            .AsQueryable();

        if (!categoryFilter.IncludeHidden)
        {
            query = query.Where(category => !category.IsHidden);
        }

        query = categoryFilter.IncludeDeleted
            ? query.IgnoreQueryFilters()
            : query.Where(category => !category.IsDeleted);

        var result = await query
            .GetViewRm()
            .ApplyOrderBy(categoryFilter, x => x.Name)
            .ToListAsync();

        return result.AsReadOnly();
    }

    public async Task<CategoryViewRm?> GetCategoryAsync(string categoryAlias, bool includeDeleted)
    {
        return await _applicationDbContext.Categories
            .Where(c => c.Alias == categoryAlias && (includeDeleted || !c.IsDeleted))
            .GetViewRm()
            .FirstOrDefaultAsync();
    }

    public async Task<int> CreateCategoryAsync(CategoryCreateRequestRm categoryCreateRequest)
    {
        var user = _userContext.GetRequiredUser();
        var utcNow = DateTime.UtcNow;
        var boardId = await _applicationDbContext.Boards.Select(x => x.Id).FirstAsync();

        var category = new Category
        {
            CreatedAt = utcNow,
            Alias = categoryCreateRequest.Alias,
            Name = categoryCreateRequest.Name,
            IsHidden = categoryCreateRequest.IsHidden,
            DefaultBumpLimit = categoryCreateRequest.DefaultBumpLimit,
            DefaultShowThreadLocalUserHash = categoryCreateRequest.DefaultShowThreadLocalUserHash,
            BoardId = boardId,
            CreatedById = user.Id,
        };
        await _applicationDbContext.Categories.AddAsync(category);
        await _applicationDbContext.SaveChangesAsync();

        return category.Id;
    }

    public async Task EditCategoryAsync(CategoryEditRequestRm categoryEditRequest)
    {
        var user = _userContext.GetRequiredUser();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        await _applicationDbContext.Categories
            .Where(c => c.Id == categoryEditRequest.Id)
            .ExecuteUpdateAsync(setProp => setProp
                .SetProperty(c => c.Alias, categoryEditRequest.Alias)
                .SetProperty(c => c.Name, categoryEditRequest.Name)
                .SetProperty(c => c.IsHidden, categoryEditRequest.IsHidden)
                .SetProperty(c => c.DefaultBumpLimit, categoryEditRequest.DefaultBumpLimit)
                .SetProperty(c => c.DefaultShowThreadLocalUserHash, categoryEditRequest.DefaultShowThreadLocalUserHash)
                .SetProperty(c => c.ModifiedAt, utcNow)
                .SetProperty(c => c.ModifiedById, user.Id));
    }

    public async Task SetCategoryDeletedAsync(int categoryId, bool isDeleted)
    {
        var user = _userContext.GetRequiredUser();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        await _applicationDbContext.Categories
            .Where(category => category.Id == categoryId)
            .ExecuteUpdateAsync(setProp =>
                setProp
                    .SetProperty(ban => ban.IsDeleted, isDeleted)
                    .SetProperty(ban => ban.ModifiedAt, utcNow)
                    .SetProperty(ban => ban.ModifiedById, user.Id));
    }
}
