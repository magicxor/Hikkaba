﻿using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Mappings;
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
        CategoryFilter filter,
        CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.CategorySource.StartActivity();

        var query = _applicationDbContext.Categories
            .TagWithCallSite()
            .Include(category => category.CreatedBy)
            .Include(category => category.ModifiedBy)
            .AsQueryable();

        if (!filter.IncludeHidden)
        {
            query = query.Where(category => !category.IsHidden);
        }

        query = filter.IncludeDeleted
            ? query.IgnoreQueryFilters()
            : query.Where(category => !category.IsDeleted);

        var result = await query
            .GetDetailsModel()
            .ApplyOrderBy(filter, x => x.Name)
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
        CategoryCreateRequestModel requestModel,
        CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.CategorySource.StartActivity();

        var user = _userContext.GetRequiredUser();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var category = requestModel.ToEntity();
        category.CreatedAt = utcNow;
        category.CreatedById = user.Id;

        _applicationDbContext.Categories.Add(category);
        await _applicationDbContext.SaveChangesAsync(cancellationToken);

        return category.Id;
    }

    public async Task EditCategoryAsync(
        CategoryEditRequestModel requestModel,
        CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.CategorySource.StartActivity();

        var user = _userContext.GetRequiredUser();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var category = await _applicationDbContext.Categories
            .TagWithCallSite()
            .FirstAsync(c => c.Id == requestModel.Id, cancellationToken);

        category.ApplyUpdate(requestModel);
        category.ModifiedAt = utcNow;
        category.ModifiedById = user.Id;

        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetCategoryModeratorsAsync(string alias, IReadOnlyList<int> moderators, CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.CategorySource.StartActivity();

        var category = await _applicationDbContext.Categories
            .TagWithCallSite()
            .Include(c => c.Moderators)
            .FirstAsync(c => c.Alias == alias, cancellationToken);

        _applicationDbContext.RemoveRange(category.Moderators);

        var newCategoryModerators = moderators
            .Select(moderatorId => new CategoryToModerator
            {
                CategoryId = category.Id,
                ModeratorId = moderatorId,
            })
            .ToList();

        _applicationDbContext.AddRange(newCategoryModerators);

        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetCategoryDeletedAsync(string alias, bool isDeleted, CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.CategorySource.StartActivity();

        var user = _userContext.GetRequiredUser();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var category = await _applicationDbContext.Categories
            .TagWithCallSite()
            .OrderBy(c => c.Id)
            .FirstAsync(c => c.Alias == alias, cancellationToken);

        category.IsDeleted = isDeleted;
        category.ModifiedAt = utcNow;
        category.ModifiedById = user.Id;

        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }
}
