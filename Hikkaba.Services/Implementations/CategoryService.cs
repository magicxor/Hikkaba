using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Extensions;
using Hikkaba.Models.Configuration;
using Hikkaba.Models.Dto;
using Hikkaba.Services.Contracts;
using Hikkaba.Services.Implementations.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Hikkaba.Services.Implementations;

public class CategoryService : BaseEntityService, ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _memoryCache;
    private readonly IOptions<HikkabaConfiguration> _options;

    public CategoryService(IMapper mapper,
        ApplicationDbContext context,
        IMemoryCache memoryCache,
        IOptions<HikkabaConfiguration> options) : base(mapper)
    {
        _context = context;
        _memoryCache = memoryCache;
        _options = options;
    }

    private IQueryable<Category> Query<TOrderKey>(Expression<Func<Category, bool>> where = null, Expression<Func<Category, TOrderKey>> orderBy = null, bool isDescending = false)
    {
        var query = _context.Categories.AsQueryable();

        if (where != null)
        {
            query = query.Where(where);
        }

        if (orderBy != null)
        {
            if (isDescending)
            {
                query = query.OrderByDescending(orderBy);
            }
            else
            {
                query = query.OrderBy(orderBy);
            }
        }

        return query;
    }

    private async Task<IList<CategoryDto>> ListNonCachedAsync<TOrderKey>(Expression<Func<Category, bool>> where, Expression<Func<Category, TOrderKey>> orderBy, bool isDescending)
    {
        var query = Query(where, orderBy, isDescending);
        var entityList = await query.ToListAsync();
        var dtoList = MapEntityListToDtoList<CategoryDto, Category>(entityList);
        return dtoList;
    }

    public async Task<IList<CategoryDto>> ListAsync<TOrderKey>(Expression<Func<Category, bool>> where = null, Expression<Func<Category, TOrderKey>> orderBy = null, bool isDescending = false)
    {
        return await _memoryCache.GetOrCreateAsync(Defaults.CacheKeyCategories,
            async x =>
            {
                x.SetAbsoluteExpiration(TimeSpan.FromSeconds(_options.Value.CacheCategoriesExpirationSeconds));
                return await ListNonCachedAsync(where, orderBy, isDescending);
            });
    }

    public async Task<BasePagedList<CategoryDto>> PagedListAsync<TOrderKey>(Expression<Func<Category, bool>> where = null, Expression<Func<Category, TOrderKey>> orderBy = null, bool isDescending = false, PageDto page = null)
    {
        page ??= new PageDto();

        var query = Query(where, orderBy, isDescending);

        var pageQuery = query.Skip(page.Skip).Take(page.PageSize);

        var entityList = await pageQuery.ToListAsync();
        var dtoList = MapEntityListToDtoList<CategoryDto, Category>(entityList);
        var pagedList = new BasePagedList<CategoryDto>
        {
            TotalItemsCount = query.Count(),
            CurrentPage = page,
            CurrentPageItems = dtoList,
        };
        return pagedList;
    }

    public async Task<CategoryDto> GetAsync(TPrimaryKey id)
    {
        var entity = await _context.Categories.FirstOrDefaultAsync(e => e.Id == id);
        var dto = MapEntityToDto<CategoryDto, Category>(entity);
        return dto;
    }

    public async Task<CategoryDto> GetAsync(string alias)
    {
        var entity = await _context.Categories.FirstOrDefaultAsync(e => e.Alias == alias);
        var dto = MapEntityToDto<CategoryDto, Category>(entity);
        return dto;
    }

    public async Task<TPrimaryKey> CreateAsync(CategoryDto dto)
    {
        var entity = MapDtoToNewEntity<CategoryDto, Category>(dto);
        entity.Board = _context.GetLocalOrAttach<Board>(dto.BoardId);
        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task EditAsync(CategoryDto dto)
    {
        var existingEntity = await _context.Categories.FirstOrDefaultAsync(e => e.Id == dto.Id);
        MapDtoToExistingEntity(dto, existingEntity);
        existingEntity.Board = _context.GetLocalOrAttach<Board>(dto.BoardId);
        await _context.SaveChangesAsync();
    }

    public async Task SetIsDeletedAsync(TPrimaryKey id, bool newValue)
    {
        var entity = _context.GetLocalOrAttach<Category>(id);
        entity.IsDeleted = newValue;
        _context.Entry(entity).Property(e => e.IsDeleted).IsModified = true;
        await _context.SaveChangesAsync();
    }
}
