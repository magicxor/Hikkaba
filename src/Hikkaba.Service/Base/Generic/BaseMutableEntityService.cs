using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto.Base;
using Hikkaba.Common.Entities;
using Hikkaba.Common.Entities.Base;
using Hikkaba.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service.Base
{
    public interface IBaseMutableEntityService<TDto, TEntity, TPrimaryKey> : IBaseEntityService<TDto, TEntity, TPrimaryKey>
    {
        Task<TPrimaryKey> CreateAsync(TDto dto);
        Task<TPrimaryKey> CreateAsync(TDto dto, TPrimaryKey currentUserId);
        Task EditAsync(TDto dto, TPrimaryKey currentUserId);
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

        public virtual async Task<TPrimaryKey> CreateAsync(TDto dto)
        {
            return await CreateAsync(dto, entity =>
            {
                entity.Created = DateTime.UtcNow;
            });
        }

        public virtual async Task<TPrimaryKey> CreateAsync(TDto dto, TPrimaryKey currentUserId)
        {
            var currentUser = await GetUserEntityByIdAsync(currentUserId);
            return await CreateAsync(dto, entity =>
            {
                entity.Created = DateTime.UtcNow;
                entity.CreatedBy = currentUser;
            });
        }

        public virtual async Task EditAsync(TDto dto, TPrimaryKey currentUserId)
        {
            var currentUser = await GetUserEntityByIdAsync(currentUserId);
            await EditAsync(dto, entity =>
            {
                entity.Modified = DateTime.UtcNow;
                entity.ModifiedBy = currentUser;
            });
        }

        public virtual async Task DeleteAsync(TPrimaryKey id)
        {
            await DeleteAsync(id, entity =>
            {
                entity.IsDeleted = true;
                entity.Modified = DateTime.UtcNow;
            });
        }

        public virtual async Task DeleteAsync(TPrimaryKey id, TPrimaryKey currentUserId)
        {
            var currentUser = await GetUserEntityByIdAsync(currentUserId);
            await DeleteAsync(id, entity =>
            {
                entity.IsDeleted = true;
                entity.Modified = DateTime.UtcNow;
                entity.ModifiedBy = currentUser;
            });
        }
    }
}