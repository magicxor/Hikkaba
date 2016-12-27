using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto.Base;
using Hikkaba.Common.Entities.Base;
using Hikkaba.Common.Exceptions;

namespace Hikkaba.Service.Base
{
    public interface IBaseImmutableEntityService<TDto, TEntity, TPrimaryKey> : IBaseEntityService<TDto, TEntity, TPrimaryKey>
    {
        Task<TPrimaryKey> CreateAsync(TDto dto);
        Task EditAsync(TDto dto);
        Task DeleteAsync(TPrimaryKey id);
    }

    public abstract class BaseImmutableEntityService<TDto, TEntity, TPrimaryKey> : BaseEntityService<TDto, TEntity, TPrimaryKey>, IBaseImmutableEntityService<TDto, TEntity, TPrimaryKey>
        where TDto : class, IBaseDto<TPrimaryKey>
        where TEntity : class, IBaseEntity<TPrimaryKey>
    {
        protected BaseImmutableEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }

        public virtual async Task<TPrimaryKey> CreateAsync(TDto dto)
        {
            return await CreateAsync(dto, entity => {});
        }

        public virtual async Task EditAsync(TDto dto)
        {
            await EditAsync(dto, entity => {});
        }
        
        public virtual async Task DeleteAsync(TPrimaryKey id)
        {
            await DeleteAsync(id, entity =>
            {
                Context.Remove(entity);
            });
        }
    }
}