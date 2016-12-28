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
    public interface IBaseEntityService<TDto, TEntity, TPrimaryKey>
    {
        Task<TDto> GetAsync(TPrimaryKey id);
        Task<IList<TDto>> ListAsync(Expression<Func<TEntity, bool>> where = null);
        Task<IList<TDto>> ListAsync<TOrderKey>(Expression<Func<TEntity, bool>> where = null, Expression<Func<TEntity, TOrderKey>> orderBy = null, bool isDescending = false);
        Task<BasePagedList<TDto>> PagedListAsync<TOrderKey>(Expression<Func<TEntity, bool>> where = null, Expression<Func<TEntity, TOrderKey>> orderBy = null, bool isDescending = false, PageDto page = null);
    }

    public abstract class BaseEntityService<TDto, TEntity, TPrimaryKey> : IBaseEntityService<TDto, TEntity, TPrimaryKey>
        where TDto : class, IBaseDto<TPrimaryKey>
        where TEntity : class, IBaseEntity<TPrimaryKey>
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
            entity.Id = entity.GenerateNewId();
            return entity;
        }

        protected TEntity MapDtoToExistingEntity(TDto dto, TEntity entity)
        {
            var resultEntity = Mapper.Map(dto, entity);
            return resultEntity;
        }
        #endregion

        protected async Task<TEntity> GetEntityByIdAsNoTrackingAsync(TPrimaryKey id)
        {
            var resultEntity = await GetDbSetWithReferences(Context).AsNoTracking().FirstOrDefaultAsync(entity => entity.Id.Equals(id));
            if (resultEntity == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound, $"{typeof(TEntity)} {id} not found.");
            }
            else
            {
                return resultEntity;
            }
        }

        protected async Task<TEntity> GetEntityByIdAsync(TPrimaryKey id)
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

        public async Task<TDto> GetAsync(TPrimaryKey id)
        {
            var entity = await GetEntityByIdAsNoTrackingAsync(id);
            return MapEntityToDto(entity);
        }

        private IQueryable<TEntity> QueryAsync<TOrderKey>(Expression<Func<TEntity, bool>> where = null, Expression<Func<TEntity, TOrderKey>> orderBy = null, bool isDescending = false)
        {
            var query = GetDbSetWithReferences(Context).AsNoTracking();

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
            return await ListAsync<bool>(where);
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

        public virtual async Task<TPrimaryKey> CreateAsync(TDto dto, Action<TEntity> setProperties, Action<TEntity> setForeignKeys)
        {
            if (dto == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, $"{nameof(dto)} is null.");
            }

            var entity = MapDtoToNewEntity(dto);
            setForeignKeys?.Invoke(entity);
            setProperties?.Invoke(entity);

            await GetDbSet(Context).AddAsync(entity);
            await Context.SaveChangesAsync();

            return entity.Id; // todo: add abstract method to retreive key
        }
        
        public virtual async Task EditAsync(TDto dto, Action<TEntity> setProperties, Action<TEntity> setForeignKeys)
        {
            if (dto == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, $"{nameof(dto)} is null.");
            }
            else if (dto.Id.Equals(default(TPrimaryKey)))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, $"{nameof(dto)}.{nameof(dto.Id)} is default or empty.");
            }

            var entity = await GetEntityByIdAsync(dto.Id);
            MapDtoToExistingEntity(dto, entity);
            setForeignKeys?.Invoke(entity);
            setProperties?.Invoke(entity);

            await Context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(TPrimaryKey id, Action<TEntity> setProperties, Action<TEntity> setForeignKeys)
        {
            if (id.Equals(default(TPrimaryKey)))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, $"{nameof(id)} is default or empty.");
            }

            var entity = await GetEntityByIdAsync(id);
            setForeignKeys?.Invoke(entity);
            setProperties?.Invoke(entity);
            
            await Context.SaveChangesAsync();
        }
    }
}