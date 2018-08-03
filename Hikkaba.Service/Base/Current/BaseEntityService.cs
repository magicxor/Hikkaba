using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities.Base.Current;
using Hikkaba.Models.Dto.Base.Current;
using Hikkaba.Service.Base.Concrete.Guid;

namespace Hikkaba.Service.Base.Current
{
    public interface IBaseEntityService<TDto, TEntity> : IBaseGuidEntityService<TDto, TEntity> { }
    public abstract class BaseEntityService<TDto, TEntity> : BaseGuidEntityService<TDto, TEntity>, IBaseEntityService<TDto, TEntity>
        where TDto : class, IBaseDto
        where TEntity : class, IBaseEntity
    {
        public BaseEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }
}