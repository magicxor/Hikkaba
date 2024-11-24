using TPrimaryKey = System.Guid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Extensions;
using Hikkaba.Models.Dto;
using Hikkaba.Services.Base.Generic;
using Microsoft.EntityFrameworkCore;
using Hikkaba.Models.Enums;

namespace Hikkaba.Services;

public interface IPostService
{
    Task<PostDto> GetAsync(TPrimaryKey id);
        
    Task<IList<PostDto>> ListAsync<TOrderKey>(
        Expression<Func<Post, bool>> where = null, 
        Expression<Func<Post, TOrderKey>> orderBy = null, 
        bool isDescending = false);

    Task<BasePagedList<PostDto>> PagedListAsync<TOrderKey>(
        Expression<Func<Post, bool>> where = null,
        Expression<Func<Post, TOrderKey>> orderBy = null,
        AdditionalRecordType additionalRecordType = AdditionalRecordType.None,
        bool isDescending = false,
        PageDto page = null);
        
    Task EditAsync(PostDto dto);
        
    Task SetIsDeletedAsync(TPrimaryKey id, bool newValue);
}

public class PostService : BaseEntityService, IPostService
{
    private readonly ApplicationDbContext _context;
        
    public PostService(IMapper mapper,
        ApplicationDbContext context) : base(mapper)
    {
        _context = context;
    }

    private IQueryable<Post> Sort<TOrderKey>(IQueryable<Post> query, Expression<Func<Post, TOrderKey>> orderBy, bool isDescending)
    {
        return isDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
    }

    private IQueryable<Post> Query<TOrderKey>(Expression<Func<Post, bool>> where = null, Expression<Func<Post, TOrderKey>> orderBy = null, bool isDescending = false)
    {
        var query = _context.Posts.Include(e => e.Attachments).AsQueryable();

        if (where != null)
        {
            query = query.Where(where);
        }

        if (orderBy != null)
        {
            query = Sort(query, orderBy, isDescending);
        }

        return query;
    }
        
    public async Task<PostDto> GetAsync(TPrimaryKey id)
    {
        var entity = await _context.Posts.FirstOrDefaultAsync(e => e.Id == id);
        var dto = MapEntityToDto<PostDto, Post>(entity);
        return dto;
    }
        
    public async Task<IList<PostDto>> ListAsync<TOrderKey>(Expression<Func<Post, bool>> where = null, Expression<Func<Post, TOrderKey>> orderBy = null, bool isDescending = false)
    {
        var query = Query(where, orderBy, isDescending);
        var entityList = await query.ToListAsync();
        var dtoList = MapEntityListToDtoList<PostDto, Post>(entityList);
        return dtoList;
    }
        
    public async Task<BasePagedList<PostDto>> PagedListAsync<TOrderKey>(Expression<Func<Post, bool>> where = null, Expression<Func<Post, TOrderKey>> orderBy = null, AdditionalRecordType additionalRecordType = AdditionalRecordType.None, bool isDescending = false, PageDto page = null)
    {
        page ??= new PageDto();

        var query = Query(where, orderBy, isDescending);

        var pageQuery = query.Skip(page.Skip).Take(page.PageSize);

        switch (additionalRecordType)
        {
            case AdditionalRecordType.First:
                pageQuery = query.Take(1).Union(pageQuery);
                break;
            case AdditionalRecordType.Last:
                pageQuery = Sort(query, orderBy, !isDescending).Take(1).Union(pageQuery);
                break;
        }

        var entityList = await pageQuery.ToListAsync();
        var dtoList = MapEntityListToDtoList<PostDto, Post>(entityList);
        var pagedList = new BasePagedList<PostDto>
        {
            TotalItemsCount = query.Count(),
            CurrentPage = page,
            CurrentPageItems = dtoList,
        };
        return pagedList;
    }
        
    public async Task EditAsync(PostDto dto)
    {
        var existingEntity = await _context.Posts.FirstOrDefaultAsync(e => e.Id == dto.Id);
        MapDtoToExistingEntity(dto, existingEntity);
        await _context.SaveChangesAsync();
    }
        
    public async Task SetIsDeletedAsync(TPrimaryKey id, bool newValue)
    {
        var entity = _context.GetLocalOrAttach<Post>(id);
        entity.IsDeleted = newValue;
        _context.Entry(entity).Property(e => e.IsDeleted).IsModified = true;
        await _context.SaveChangesAsync();
    }
}