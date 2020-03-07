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

namespace Hikkaba.Services
{
    public interface IApplicationUserService
    {
        Task<ApplicationUserDto> GetAsync(TPrimaryKey id);
        
        Task<IList<ApplicationUserDto>> ListAsync<TOrderKey>(
            Expression<Func<ApplicationUser, bool>> where = null, 
            Expression<Func<ApplicationUser, TOrderKey>> orderBy = null, 
            bool isDescending = false);

        Task<BasePagedList<ApplicationUserDto>> PagedListAsync<TOrderKey>(
            Expression<Func<ApplicationUser, bool>> where = null,
            Expression<Func<ApplicationUser, TOrderKey>> orderBy = null, bool isDescending = false,
            PageDto page = null);
        
        Task<TPrimaryKey> CreateAsync(ApplicationUserDto dto);
        
        Task EditAsync(ApplicationUserDto dto);
        
        Task DeleteAsync(TPrimaryKey id);
    }
    
    public class ApplicationUserService : BaseEntityService, IApplicationUserService
    {
        private readonly ApplicationDbContext _context;

        public ApplicationUserService(IMapper mapper, ApplicationDbContext context) : base(mapper)
        {
            _context = context;
        }
        
        private IQueryable<ApplicationUser> Query<TOrderKey>(Expression<Func<ApplicationUser, bool>> where = null, Expression<Func<ApplicationUser, TOrderKey>> orderBy = null, bool isDescending = false)
        {
            var query = _context.Users.AsQueryable();

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
        
        public async Task<ApplicationUserDto> GetAsync(TPrimaryKey id)
        {
            var entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            var dto = MapEntityToDto<ApplicationUserDto, ApplicationUser>(entity);
            return dto;
        }
        
        public async Task<IList<ApplicationUserDto>> ListAsync<TOrderKey>(Expression<Func<ApplicationUser, bool>> where = null, Expression<Func<ApplicationUser, TOrderKey>> orderBy = null, bool isDescending = false)
        {
            var query = Query(where, orderBy, isDescending);
            var entityList = await query.ToListAsync();
            var dtoList = MapEntityListToDtoList<ApplicationUserDto, ApplicationUser>(entityList);
            return dtoList;
        }

        public async Task<BasePagedList<ApplicationUserDto>> PagedListAsync<TOrderKey>(Expression<Func<ApplicationUser, bool>> where = null, Expression<Func<ApplicationUser, TOrderKey>> orderBy = null, bool isDescending = false, PageDto page = null)
        {
            page = page ?? new PageDto();

            var query = Query(where, orderBy, isDescending);

            var pageQuery = query.Skip(page.Skip).Take(page.PageSize);

            var entityList = await pageQuery.ToListAsync();
            var dtoList = MapEntityListToDtoList<ApplicationUserDto, ApplicationUser>(entityList);
            var pagedList = new BasePagedList<ApplicationUserDto>
            {
                TotalItemsCount = query.Count(),
                CurrentPage = page,
                CurrentPageItems = dtoList,
            };
            return pagedList;
        }
        
        public async Task<TPrimaryKey> CreateAsync(ApplicationUserDto dto)
        {
            var entity = MapDtoToNewEntity<ApplicationUserDto, ApplicationUser>(dto);
            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task EditAsync(ApplicationUserDto dto)
        {
            var existingEntity = await _context.Users.FirstOrDefaultAsync(user => user.Id == dto.Id);
            MapDtoToExistingEntity(dto, existingEntity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TPrimaryKey id)
        {
            var entity = _context.GetLocalOrAttach<ApplicationUser>(id);
            entity.IsDeleted = true;
            entity.LockoutEnabled = true;
            _context.Entry(entity).Property(e => e.IsDeleted).IsModified = true;
            _context.Entry(entity).Property(e => e.LockoutEnabled).IsModified = true;
            await _context.SaveChangesAsync();
        }
    }
}