using TPrimaryKey = System.Guid;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Base.Current;
using Hikkaba.Models.Dto.Base.Current;
using Hikkaba.Services.Base.Generic;
using Microsoft.AspNetCore.Identity;

namespace Hikkaba.Services.Base.Current
{
    public interface IBaseEntityService<TDto, TEntity> : IBaseEntityService<TDto, TEntity, TPrimaryKey> { }
    public abstract class BaseEntityService<TDto, TEntity> : BaseEntityService<TDto, TEntity, TPrimaryKey>, IBaseEntityService<TDto, TEntity>
        where TDto : class, IBaseDto
        where TEntity : class, IBaseEntity
    {
        public BaseEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }

    public interface IBaseImmutableEntityService<TDto, TEntity> : IBaseImmutableEntityService<TDto, TEntity, TPrimaryKey> { }
    public abstract class BaseImmutableEntityService<TDto, TEntity> :
        BaseImmutableEntityService<TDto, TEntity, TPrimaryKey>, IBaseImmutableEntityService<TDto, TEntity>
        where TDto : class, IBaseDto
        where TEntity : class, IBaseEntity
    {
        public BaseImmutableEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }

    public interface IBaseModeratedMutableEntityService<TDto, TEntity> : IBaseModeratedMutableEntityService<TDto, TEntity, TPrimaryKey> { }
    public abstract class BaseModeratedMutableEntityService<TDto, TEntity> :
        BaseModeratedMutableEntityService<TDto, TEntity, TPrimaryKey>,
        IBaseModeratedMutableEntityService<TDto, TEntity>
        where TDto : class, IBaseMutableDto
        where TEntity : class, IBaseMutableEntity
    {
        public BaseModeratedMutableEntityService(IMapper mapper, ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(mapper, context, userManager)
        {
        }
    }

    public interface IBaseMutableEntityService<TDto, TEntity> : IBaseMutableEntityService<TDto, TEntity, TPrimaryKey> { }
    public abstract class BaseMutableEntityService<TDto, TEntity> : BaseMutableEntityService<TDto, TEntity, TPrimaryKey>, IBaseMutableEntityService<TDto, TEntity>
        where TDto : class, IBaseMutableDto
        where TEntity : class, IBaseMutableEntity
    {
        public BaseMutableEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }

    public interface IBaseManyToManyService : IBaseManyToManyService<TPrimaryKey, TPrimaryKey> { }
    public abstract class BaseManyToManyService<TManyToManyEntity> : BaseManyToManyService<TManyToManyEntity, TPrimaryKey, TPrimaryKey>, IBaseManyToManyService
        where TManyToManyEntity : class
    {
        public BaseManyToManyService(ApplicationDbContext context) : base(context)
        {
        }
    }
}
