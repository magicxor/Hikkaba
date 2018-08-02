using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Base.Current;
using Hikkaba.Models.Dto.Base.Current;
using Hikkaba.Service.Base.Generic;
using Microsoft.AspNetCore.Identity;

namespace Hikkaba.Service.Base.Concrete.Guid
{
    public interface IBaseGuidModeratedMutableEntityService<TDto, TEntity> : IBaseModeratedMutableEntityService<TDto, TEntity, System.Guid>{}
    public abstract class BaseGuidModeratedMutableEntityService<TDto, TEntity> :
        BaseModeratedMutableEntityService<TDto, TEntity, System.Guid>,
        IBaseGuidModeratedMutableEntityService<TDto, TEntity>
        where TDto : class, IBaseMutableDto
        where TEntity : class, IBaseMutableEntity
    {
        public BaseGuidModeratedMutableEntityService(IMapper mapper, ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(mapper, context, userManager)
        {
        }
    }
}
