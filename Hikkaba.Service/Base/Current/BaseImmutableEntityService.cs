using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities.Base.Current;
using Hikkaba.Models.Dto.Base.Current;
using Hikkaba.Service.Base.Concrete.Guid;

namespace Hikkaba.Service.Base.Current
{
    public interface IBaseImmutableEntityService<TDto, TEntity> : IBaseGuidImmutableEntityService<TDto, TEntity> { }
    public abstract class BaseImmutableEntityService<TDto, TEntity> :
        BaseGuidImmutableEntityService<TDto, TEntity>,
        IBaseImmutableEntityService<TDto, TEntity>
        where TDto : class, IBaseDto
        where TEntity : class, IBaseEntity
    {
        public BaseImmutableEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }
}