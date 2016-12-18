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

        public async Task<Guid> CreateAsync(TDto dto)
        {
            if (dto == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, $"{nameof(dto)} is null.");
            }

            var entity = MapDtoToNewEntity(dto);
            
            await GetDbSet(Context).AddAsync(entity);
            await Context.SaveChangesAsync();

            return entity.Id;
        }

        public async Task DeleteAsync(Guid id)
        {
            if (id == default(Guid) || id == Guid.Empty)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, $"{nameof(id)} is default or empty.");
            }

            var entity = await GetEntityByIdAsync(id);
            Context.Remove(entity);
            await Context.SaveChangesAsync();
        }
    }
}