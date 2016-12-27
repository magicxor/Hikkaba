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