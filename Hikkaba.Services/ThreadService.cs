using TPrimaryKey = System.Guid;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Hikkaba.Services.Base.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using Hikkaba.Data.Extensions;

namespace Hikkaba.Services
{
    public interface IThreadService
    {
        Task<ThreadDto> GetAsync(TPrimaryKey id);
        
        Task<IList<ThreadDto>> ListAsync<TOrderKey>(
            Expression<Func<Thread, bool>> where = null, 
            Expression<Func<Thread, TOrderKey>> orderBy = null, 
            bool isDescending = false);

        Task<TPrimaryKey> CreateAsync(ThreadDto dto);
        
        Task EditAsync(ThreadDto dto);
        
        Task<BasePagedList<ThreadDto>> PagedListAsync(Expression<Func<Thread, bool>> where, PageDto page = null);
        
        Task DeleteAsync(TPrimaryKey id);
    }

    public class ThreadService : BaseEntityService, IThreadService
    {
        private readonly ApplicationDbContext _context;

        public ThreadService(IMapper mapper,
            ApplicationDbContext context) : base(mapper)
        {
            _context = context;
        }
        
        private IQueryable<Thread> Query<TOrderKey>(Expression<Func<Thread, bool>> where = null, Expression<Func<Thread, TOrderKey>> orderBy = null, bool isDescending = false)
        {
            var query = _context.Threads.AsQueryable();

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
        
        public async Task<ThreadDto> GetAsync(TPrimaryKey id)
        {
            var entity = await _context.Threads.FirstOrDefaultAsync(u => u.Id == id);
            var dto = MapEntityToDto<ThreadDto, Thread>(entity);
            return dto;
        }

        public async Task<IList<ThreadDto>> ListAsync<TOrderKey>(Expression<Func<Thread, bool>> where = null, Expression<Func<Thread, TOrderKey>> orderBy = null, bool isDescending = false)
        {
            var query = Query(where, orderBy, isDescending);
            var entityList = await query.ToListAsync();
            var dtoList = MapEntityListToDtoList<ThreadDto, Thread>(entityList);
            return dtoList;
        }
        
        public async Task<TPrimaryKey> CreateAsync(ThreadDto dto)
        {
            var entity = MapDtoToNewEntity<ThreadDto, Thread>(dto);
            entity.Category = _context.GetLocalOrAttach<Category>(dto.CategoryId);
            await _context.Threads.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }
        
        public async Task EditAsync(ThreadDto dto)
        {
            var existingEntity = await _context.Threads.FirstOrDefaultAsync(t => t.Id == dto.Id);
            MapDtoToExistingEntity(dto, existingEntity);
            existingEntity.Category = _context.GetLocalOrAttach<Category>(dto.CategoryId);
            await _context.SaveChangesAsync();
        }
        
        public async Task<BasePagedList<ThreadDto>> PagedListAsync(Expression<Func<Thread, bool>> where, PageDto page = null)
        {
            page = page ?? new PageDto();

            var query = _context.Threads
                .Where(where)
                .OrderByDescending(thread => thread.IsPinned)
                .ThenByDescending(thread => thread.Posts
                    .OrderBy(post => post.Created)
                    .Take(thread.BumpLimit).
                    OrderByDescending(post => post.Created)
                    .FirstOrDefault(post => (!post.IsSageEnabled) && (!post.IsDeleted))
                    .Created);

            var pagedQuery = query
                .Skip(page.Skip)
                .Take(page.PageSize);

            var entityList = await pagedQuery.ToListAsync();
            var dtoList = MapEntityListToDtoList<ThreadDto, Thread>(entityList);
            var pagedList = new BasePagedList<ThreadDto>
            {
                CurrentPage = page,
                TotalItemsCount = query.Count(),
                CurrentPageItems = dtoList
            };
            return pagedList;
        }
        
        public async Task DeleteAsync(TPrimaryKey id)
        {
            var entity = _context.GetLocalOrAttach<Thread>(id);
            entity.IsDeleted = true;
            _context.Entry(entity).Property(e => e.IsDeleted).IsModified = true;
            await _context.SaveChangesAsync();
        }
    }
}
