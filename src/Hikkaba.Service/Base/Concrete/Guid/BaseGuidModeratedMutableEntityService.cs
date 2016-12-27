using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Constants;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Dto.Base;
using Hikkaba.Common.Entities;
using Hikkaba.Common.Entities.Base;
using Hikkaba.Common.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service.Base
{
    public interface IBaseGuidModeratedMutableEntityService<TDto, TEntity> : IBaseModeratedMutableEntityService<TDto, TEntity, Guid>{}
    public abstract class BaseGuidModeratedMutableEntityService<TDto, TEntity> :
        BaseModeratedMutableEntityService<TDto, TEntity, Guid>,
        IBaseGuidModeratedMutableEntityService<TDto, TEntity>
        where TDto : class, IBaseMutableDto
        where TEntity : class, IBaseMutableEntity
    {
        public BaseGuidModeratedMutableEntityService(IMapper mapper, ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(mapper, context, userManager)
        {
        }
    }
}
