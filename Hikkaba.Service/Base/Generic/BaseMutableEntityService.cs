using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Base.Generic;
using Hikkaba.Infrastructure.Exceptions;
using Hikkaba.Models.Dto.Base.Generic;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service.Base.Generic
{
    public interface IBaseMutableEntityService<TDto, TEntity, TPrimaryKey> : IBaseEntityService<TDto, TEntity, TPrimaryKey>
    {
        Task<TPrimaryKey> CreateAsync(TDto dto, Action<TEntity> setForeignKeys);
        Task<TPrimaryKey> CreateAsync(TDto dto, TPrimaryKey currentUserId, Action<TEntity> setForeignKeys);
        Task EditAsync(TDto dto, TPrimaryKey currentUserId, Action<TEntity> setForeignKeys);
        Task DeleteAsync(TPrimaryKey id);
        Task DeleteAsync(TPrimaryKey id, TPrimaryKey currentUserId);
    }

    public abstract class BaseMutableEntityService<TDto, TEntity, TPrimaryKey> : BaseEntityService<TDto, TEntity, TPrimaryKey>, IBaseMutableEntityService<TDto, TEntity, TPrimaryKey>
        where TDto : class, IBaseMutableDto<TPrimaryKey> 
        where TEntity : class, IBaseMutableEntity<TPrimaryKey>
    {
        protected BaseMutableEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }

        protected async Task<ApplicationUser> GetUserEntityByIdAsync(TPrimaryKey userId)
        {
            var userEntity = await Context.Users.FirstOrDefaultAsync(user => user.Id.Equals(userId));
            if (userEntity == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound, $"User {userId} not found.");
            }
            else
            {
                return userEntity;
            }
        }

        public virtual async Task<TPrimaryKey> CreateAsync(TDto dto, Action<TEntity> setForeignKeys)
        {
            return await base.CreateAsync(dto, entity =>
            {
                entity.Created = DateTime.UtcNow;
            }, setForeignKeys);
        }

        public virtual async Task<TPrimaryKey> CreateAsync(TDto dto, TPrimaryKey currentUserId, Action<TEntity> setForeignKeys)
        {
            var currentUser = await GetUserEntityByIdAsync(currentUserId);
            return await base.CreateAsync(dto, entity =>
            {
                entity.Created = DateTime.UtcNow;
                entity.CreatedBy = currentUser;
            }, setForeignKeys);
        }

        public virtual async Task EditAsync(TDto dto, TPrimaryKey currentUserId, Action<TEntity> setForeignKeys)
        {
            var currentUser = await GetUserEntityByIdAsync(currentUserId);
            await base.EditAsync(dto, entity =>
            {
                entity.Modified = DateTime.UtcNow;
                entity.ModifiedBy = currentUser;
            }, setForeignKeys);
        }

        public virtual async Task DeleteAsync(TPrimaryKey id)
        {
            await base.DeleteAsync(id, entity =>
            {
                entity.IsDeleted = true;
                entity.Modified = DateTime.UtcNow;
            }, entity => { });
        }

        public virtual async Task DeleteAsync(TPrimaryKey id, TPrimaryKey currentUserId)
        {
            var currentUser = await GetUserEntityByIdAsync(currentUserId);
            await base.DeleteAsync(id, entity =>
            {
                entity.IsDeleted = true;
                entity.Modified = DateTime.UtcNow;
                entity.ModifiedBy = currentUser;
            }, entity => { });
        }
    }
}