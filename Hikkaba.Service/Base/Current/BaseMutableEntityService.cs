using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities.Base.Current;
using Hikkaba.Models.Dto.Base.Current;
using Hikkaba.Service.Base.Concrete.Guid;

namespace Hikkaba.Service.Base.Current
{
    public interface IBaseMutableEntityService<TDto, TEntity> : IBaseGuidMutableEntityService<TDto, TEntity> { }
    public abstract class BaseMutableEntityService<TDto, TEntity> :
        BaseGuidMutableEntityService<TDto, TEntity>, 
        IBaseMutableEntityService<TDto, TEntity>
        where TDto : class, IBaseMutableDto
        where TEntity : class, IBaseMutableEntity
    {
        public BaseMutableEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }
}