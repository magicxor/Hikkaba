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
    public interface IBaseImmutableEntityService<TDto, TEntity> : IBaseEntityService<TDto, TEntity>
    {
        Task<Guid> CreateAsync(TDto dto);
        Task DeleteAsync(Guid id);
    }

    public abstract class BaseImmutableEntityService<TDto, TEntity> : BaseEntityService<TDto, TEntity>, IBaseImmutableEntityService<TDto, TEntity>
        where TDto : BaseDto 
        where TEntity : BaseEntity
    {
        protected BaseImmutableEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }

        public virtual async Task<Guid> CreateAsync(TDto dto)
        {
            return await CreateAsync(dto, entity => {});
        }

        public virtual async Task EditAsync(TDto dto)
        {
            await EditAsync(dto, entity => {});
        }
        
        public virtual async Task DeleteAsync(Guid id)
        {
            await DeleteAsync(id, entity =>
            {
                Context.Remove(entity);
            });
        }
    }
}