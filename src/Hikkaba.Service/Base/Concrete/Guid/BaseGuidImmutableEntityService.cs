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
    public interface IBaseGuidImmutableEntityService<TDto, TEntity> : IBaseImmutableEntityService<TDto, TEntity, Guid> { }
    public abstract class BaseGuidImmutableEntityService<TDto, TEntity> :
        BaseImmutableEntityService<TDto, TEntity, Guid>, IBaseGuidImmutableEntityService<TDto, TEntity>
        where TDto : class, IBaseDto
        where TEntity : class, IBaseEntity
    {
        public BaseGuidImmutableEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }
}