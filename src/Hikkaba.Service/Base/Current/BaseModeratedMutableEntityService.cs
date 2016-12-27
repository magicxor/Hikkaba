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
