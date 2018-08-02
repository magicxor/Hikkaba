using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities.Base.Current;
using Hikkaba.Models.Dto.Base.Current;
using Hikkaba.Service.Base.Generic;

namespace Hikkaba.Service.Base.Concrete.Guid
{
    public interface IBaseGuidImmutableEntityService<TDto, TEntity> : IBaseImmutableEntityService<TDto, TEntity, System.Guid> { }
    public abstract class BaseGuidImmutableEntityService<TDto, TEntity> :
        BaseImmutableEntityService<TDto, TEntity, System.Guid>, IBaseGuidImmutableEntityService<TDto, TEntity>
        where TDto : class, IBaseDto
        where TEntity : class, IBaseEntity
    {
        public BaseGuidImmutableEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }
}