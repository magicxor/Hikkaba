using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities.Base.Current;
using Hikkaba.Models.Dto.Base.Current;
using Hikkaba.Service.Base.Generic;

namespace Hikkaba.Service.Base.Concrete.Guid
{
    public interface IBaseGuidEntityService<TDto, TEntity> : IBaseEntityService<TDto, TEntity, System.Guid> { }
    public abstract class BaseGuidEntityService<TDto, TEntity> : BaseEntityService<TDto, TEntity, System.Guid>, IBaseGuidEntityService<TDto, TEntity>
        where TDto : class, IBaseDto
        where TEntity : class, IBaseEntity
    {
        public BaseGuidEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }
}