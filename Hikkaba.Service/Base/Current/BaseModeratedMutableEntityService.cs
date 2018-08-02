using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Base.Current;
using Hikkaba.Models.Dto.Base.Current;
using Hikkaba.Service.Base.Concrete.Guid;
using Microsoft.AspNetCore.Identity;

namespace Hikkaba.Service.Base.Current
{
    public interface IBaseModeratedMutableEntityService<TDto, TEntity> : IBaseGuidModeratedMutableEntityService<TDto, TEntity>{ }
    public abstract class BaseModeratedMutableEntityService<TDto, TEntity> :
        BaseGuidModeratedMutableEntityService<TDto, TEntity>,
        IBaseModeratedMutableEntityService<TDto, TEntity>
        where TDto : class, IBaseMutableDto
        where TEntity : class, IBaseMutableEntity
    {
        public BaseModeratedMutableEntityService(IMapper mapper, ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(mapper, context, userManager)
        {
        }
    }
}
