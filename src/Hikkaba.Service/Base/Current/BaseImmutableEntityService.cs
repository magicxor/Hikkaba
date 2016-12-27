using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto.Base;
using Hikkaba.Common.Entities.Base;
using Hikkaba.Common.Exceptions;

namespace Hikkaba.Service.Base
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