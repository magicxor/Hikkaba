using System;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities.Base.Generic;
using Hikkaba.Models.Dto.Base.Generic;

namespace Hikkaba.Service.Base.Generic
{
    public interface IBaseImmutableEntityService<TDto, TEntity, TPrimaryKey> : IBaseEntityService<TDto, TEntity, TPrimaryKey>
    {
        Task<TPrimaryKey> CreateAsync(TDto dto, Action<TEntity> setForeignKeys);
        Task EditAsync(TDto dto, Action<TEntity> setForeignKeys);
        Task DeleteAsync(TPrimaryKey id);
    }

    public abstract class BaseImmutableEntityService<TDto, TEntity, TPrimaryKey> : BaseEntityService<TDto, TEntity, TPrimaryKey>, IBaseImmutableEntityService<TDto, TEntity, TPrimaryKey>
        where TDto : class, IBaseDto<TPrimaryKey>
        where TEntity : class, IBaseEntity<TPrimaryKey>
    {
        protected BaseImmutableEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }

        public virtual async Task<TPrimaryKey> CreateAsync(TDto dto, Action<TEntity> setForeignKeys)
        {
            return await CreateAsync(dto, entity => {}, setForeignKeys);
        }

        public virtual async Task EditAsync(TDto dto, Action<TEntity> setForeignKeys)
        {
            await EditAsync(dto, entity => {}, setForeignKeys);
        }
        
        public virtual async Task DeleteAsync(TPrimaryKey id)
        {
            await DeleteAsync(id, entity =>
            {
                Context.Remove(entity);
            }, (entity) => {});
        }
    }
}