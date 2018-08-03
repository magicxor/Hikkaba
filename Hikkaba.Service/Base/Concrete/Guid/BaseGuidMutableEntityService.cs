using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities.Base.Current;
using Hikkaba.Models.Dto.Base.Current;
using Hikkaba.Service.Base.Generic;

namespace Hikkaba.Service.Base.Concrete.Guid
{
    public interface IBaseGuidMutableEntityService<TDto, TEntity> : IBaseMutableEntityService<TDto, TEntity, System.Guid>{}
    public abstract class BaseGuidMutableEntityService<TDto, TEntity> : BaseMutableEntityService<TDto, TEntity, System.Guid>, IBaseGuidMutableEntityService<TDto, TEntity>
        where TDto : class, IBaseMutableDto
        where TEntity : class, IBaseMutableEntity
    {
        public BaseGuidMutableEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }
}