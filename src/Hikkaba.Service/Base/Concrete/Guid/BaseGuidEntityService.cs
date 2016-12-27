using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Dto.Base;
using Hikkaba.Common.Entities.Base;
using Hikkaba.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service.Base
{
    public interface IBaseGuidEntityService<TDto, TEntity> : IBaseEntityService<TDto, TEntity, Guid> { }
    public abstract class BaseGuidEntityService<TDto, TEntity> : BaseEntityService<TDto, TEntity, Guid>, IBaseGuidEntityService<TDto, TEntity>
        where TDto : class, IBaseDto
        where TEntity : class, IBaseEntity
    {
        public BaseGuidEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }
}