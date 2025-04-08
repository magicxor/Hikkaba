using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Infrastructure.Repositories.QueryableExtensions;
using Hikkaba.Infrastructure.Repositories.Telemetry;
using Hikkaba.Paging.Extensions;
using Hikkaba.Shared.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

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

    public async Task<IReadOnlyList<CategoryDetailsModel>> ListCategoriesAsync(
        CategoryFilter categoryFilter,
        CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.CategorySource.StartActivity();

        var query = _applicationDbContext.Categories
            .TagWithCallSite()
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
            .GetDetailsModel()
            .ApplyOrderBy(categoryFilter, x => x.Name)
            .ToListAsync(cancellationToken);

        return result.AsReadOnly();
    }

    public async Task<CategoryDetailsModel?> GetCategoryAsync(string categoryAlias, bool includeDeleted, CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.CategorySource.StartActivity();

        return await _applicationDbContext.Categories
            .TagWithCallSite()
            .Where(c => c.Alias == categoryAlias && (includeDeleted || !c.IsDeleted))
            .OrderBy(c => c.Id)
            .GetDetailsModel()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CreateCategoryAsync(
        CategoryCreateRequestModel categoryCreateRequest,
        CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.CategorySource.StartActivity();

        var user = _userContext.GetRequiredUser();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var boardId = await _applicationDbContext.Boards
            .TagWithCallSite()
            .Select(x => x.Id)
            .OrderBy(x => x)
            .FirstAsync(cancellationToken);

        var category = new Category
        {
            CreatedAt = utcNow,
            Alias = categoryCreateRequest.Alias,
            Name = categoryCreateRequest.Name,
            IsHidden = categoryCreateRequest.IsHidden,
            DefaultBumpLimit = categoryCreateRequest.DefaultBumpLimit,
            ShowThreadLocalUserHash = categoryCreateRequest.ShowThreadLocalUserHash,
            ShowCountry = categoryCreateRequest.ShowCountry,
            ShowOs = categoryCreateRequest.ShowOs,
            ShowBrowser = categoryCreateRequest.ShowBrowser,
            MaxThreadCount = categoryCreateRequest.MaxThreadCount,
            BoardId = boardId,
            CreatedById = user.Id,
        };
        await _applicationDbContext.Categories.AddAsync(category, cancellationToken);
        await _applicationDbContext.SaveChangesAsync(cancellationToken);

        return category.Id;
    }

    public async Task EditCategoryAsync(
        CategoryEditRequestModel categoryEditRequest,
        CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.CategorySource.StartActivity();

        var user = _userContext.GetRequiredUser();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        await _applicationDbContext.Categories
            .TagWithCallSite()
            .Where(c => c.Id == categoryEditRequest.Id)
            .ExecuteUpdateAsync(setProp => setProp
                .SetProperty(c => c.Alias, categoryEditRequest.Alias)
                .SetProperty(c => c.Name, categoryEditRequest.Name)
                .SetProperty(c => c.IsHidden, categoryEditRequest.IsHidden)
                .SetProperty(c => c.DefaultBumpLimit, categoryEditRequest.DefaultBumpLimit)
                .SetProperty(c => c.ShowThreadLocalUserHash, categoryEditRequest.ShowThreadLocalUserHash)
                .SetProperty(c => c.ShowCountry, categoryEditRequest.ShowCountry)
                .SetProperty(c => c.ShowOs, categoryEditRequest.ShowOs)
                .SetProperty(c => c.ShowBrowser, categoryEditRequest.ShowBrowser)
                .SetProperty(c => c.MaxThreadCount, categoryEditRequest.MaxThreadCount)
                .SetProperty(c => c.ModifiedAt, utcNow)
                .SetProperty(c => c.ModifiedById, user.Id),
                cancellationToken);
    }

    public async Task SetCategoryDeletedAsync(int categoryId, bool isDeleted, CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.CategorySource.StartActivity();

        var user = _userContext.GetRequiredUser();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        await _applicationDbContext.Categories
            .TagWithCallSite()
            .Where(category => category.Id == categoryId)
            .ExecuteUpdateAsync(setProp =>
                setProp
                    .SetProperty(ban => ban.IsDeleted, isDeleted)
                    .SetProperty(ban => ban.ModifiedAt, utcNow)
                    .SetProperty(ban => ban.ModifiedById, user.Id),
                cancellationToken);
    }
}
