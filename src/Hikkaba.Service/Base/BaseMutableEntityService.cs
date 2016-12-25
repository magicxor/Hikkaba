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
    public interface IBaseMutableEntityService<TDto, TEntity, TUserKey> : IBaseEntityService<TDto, TEntity>
    {
        Task<Guid> CreateAsync(TDto dto);
        Task<Guid> CreateAsync(TDto dto, TUserKey currentUserId);
        Task EditAsync(TDto dto, TUserKey currentUserId);
        Task DeleteAsync(Guid id);
        Task DeleteAsync(Guid id, TUserKey currentUserId);
    }

    public abstract class BaseMutableEntityService<TDto, TEntity, TUserKey> : BaseEntityService<TDto, TEntity>, IBaseMutableEntityService<TDto, TEntity, TUserKey>
        where TDto : BaseMutableDto 
        where TEntity : BaseMutableEntity
    {
        protected BaseMutableEntityService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }

        protected async Task<ApplicationUser> GetUserEntityByIdAsync(TUserKey userId)
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

        public virtual async Task<Guid> CreateAsync(TDto dto)
        {
            return await CreateAsync(dto, entity =>
            {
                entity.Created = DateTime.UtcNow;
            });
        }

        public virtual async Task<Guid> CreateAsync(TDto dto, TUserKey currentUserId)
        {
            var currentUser = await GetUserEntityByIdAsync(currentUserId);
            return await CreateAsync(dto, entity =>
            {
                entity.Created = DateTime.UtcNow;
                entity.CreatedBy = currentUser;
            });
        }

        public virtual async Task EditAsync(TDto dto, TUserKey currentUserId)
        {
            var currentUser = await GetUserEntityByIdAsync(currentUserId);
            await EditAsync(dto, entity =>
            {
                entity.Modified = DateTime.UtcNow;
                entity.ModifiedBy = currentUser;
            });
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            await DeleteAsync(id, entity =>
            {
                entity.IsDeleted = true;
                entity.Modified = DateTime.UtcNow;
            });
        }

        public virtual async Task DeleteAsync(Guid id, TUserKey currentUserId)
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