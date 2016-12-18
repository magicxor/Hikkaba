using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Dto.Base;
using Hikkaba.Common.Entities.Base;
using Hikkaba.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service.Base
{
    public interface IBaseEntityService<TDto, TEntity>
    {
        Task<TDto> GetAsync(Guid id);
        Task<IList<TDto>> ListAsync(Expression<Func<TEntity, bool>> where = null);
        Task<IList<TDto>> ListAsync<TOrderKey>(Expression<Func<TEntity, bool>> where = null, Expression<Func<TEntity, TOrderKey>> orderBy = null, bool isDescending = false);
        Task<BasePagedList<TDto>> PagedListAsync<TOrderKey>(Expression<Func<TEntity, bool>> where = null, Expression<Func<TEntity, TOrderKey>> orderBy = null, bool isDescending = false, PageDto page = null);
    }

    public abstract class BaseEntityService<TDto, TEntity> : IBaseEntityService<TDto, TEntity>
        where TDto : BaseDto 
        where TEntity : BaseEntity
    {
        protected IMapper Mapper { get; set; }
        protected ApplicationDbContext Context { get; set; }
        protected abstract DbSet<TEntity> GetDbSet(ApplicationDbContext context);
        protected abstract IQueryable<TEntity> GetDbSetWithReferences(ApplicationDbContext context);
        protected abstract void LoadReferenceFields(ApplicationDbContext context, TEntity entityEntry);

        protected BaseEntityService(IMapper mapper, ApplicationDbContext context)
        {
            Mapper = mapper;
            Context = context;
        }

        protected async Task<TEntity> GetEntityByIdAsync(Guid id)
        {
            var resultEntity = await GetDbSetWithReferences(Context).FirstOrDefaultAsync(entity => entity.Id.Equals(id));
            if (resultEntity == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound, $"{typeof(TEntity)} {id} not found.");
            }
            else
            {
                return resultEntity;
            }
        }

        #region MapMethods
        protected TDto MapEntityToDto(TEntity entity)
        {
            var dto = Mapper.Map<TDto>(entity);
            return dto;
        }

        protected IList<TDto> MapEntityListToDtoList(IList<TEntity> entityList)
        {
            var dtoList = Mapper.Map<List<TDto>>(entityList);
            return dtoList;
        }

        protected TEntity MapDtoToNewEntity(TDto dto)
        {
            var entity = Mapper.Map<TEntity>(dto);
            entity.Id = Guid.NewGuid();
            return entity;
        }

        protected TEntity MapDtoToExistingEntity(TDto dto, TEntity entity)
        {
            var resultEntity = Mapper.Map(dto, entity);
            return resultEntity;
        }
        #endregion

        public async Task<TDto> GetAsync(Guid id)
        {
            var entity = await GetEntityByIdAsync(id);
            LoadReferenceFields(Context, entity);
            return MapEntityToDto(entity);
        }

        private IQueryable<TEntity> QueryAsync<TOrderKey>(Expression<Func<TEntity, bool>> where = null, Expression<Func<TEntity, TOrderKey>> orderBy = null, bool isDescending = false)
        {
            var query = GetDbSetWithReferences(Context);

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

        public async Task<IList<TDto>> ListAsync(Expression<Func<TEntity, bool>> where = null)
        {
            var query = QueryAsync<bool>(where);
            var entityList = await query.ToListAsync();
            var dtoList = MapEntityListToDtoList(entityList);
            return dtoList;
        }

        public async Task<IList<TDto>> ListAsync<TOrderKey>(Expression<Func<TEntity, bool>> where = null, Expression<Func<TEntity, TOrderKey>> orderBy = null, bool isDescending = false)
        {
            var query = QueryAsync(where, orderBy, isDescending);
            var entityList = await query.ToListAsync();
            var dtoList = MapEntityListToDtoList(entityList);
            return dtoList;
        }

        public async Task<BasePagedList<TDto>> PagedListAsync<TOrderKey>(Expression<Func<TEntity, bool>> where = null, Expression<Func<TEntity, TOrderKey>> orderBy = null, bool isDescending = false, PageDto page = null)
        {
            page = page ?? new PageDto();

            var query = QueryAsync(where, orderBy, isDescending);

            var pageQuery = query.Skip(page.Skip).Take(page.PageSize);

            var entityList = await pageQuery.ToListAsync();
            var dtoList = MapEntityListToDtoList(entityList);
            var pagedList = new BasePagedList<TDto>()
            {
                TotalItemsCount = query.Count(),
                CurrentPage = page,
                CurrentPageItems = dtoList,
            };
            return pagedList;
        }
    }
}