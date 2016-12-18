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

        public async Task<Guid> CreateAsync(TDto dto)
        {
            if (dto == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, $"{nameof(dto)} is null.");
            }

            var entity = MapDtoToNewEntity(dto);
            entity.Created = DateTime.UtcNow;

            await GetDbSet(Context).AddAsync(entity);
            await Context.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<Guid> CreateAsync(TDto dto, TUserKey currentUserId)
        {
            var currentUser = GetUserEntityByIdAsync(currentUserId);
            if (dto == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, $"{nameof(dto)} is null.");
            }

            var entity = MapDtoToNewEntity(dto);
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = await currentUser;

            await GetDbSet(Context).AddAsync(entity);
            await Context.SaveChangesAsync();

            return entity.Id;
        }

        public async Task EditAsync(TDto dto, TUserKey currentUserId)
        {
            var currentUser = GetUserEntityByIdAsync(currentUserId);
            if (dto == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, $"{nameof(dto)} is null.");
            }
            else if (dto.Id == default(Guid) || dto.Id == Guid.Empty)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, $"{nameof(dto)}.{nameof(dto.Id)} is default or empty.");
            }

            var entity = await GetEntityByIdAsync(dto.Id);
            MapDtoToExistingEntity(dto, entity);
            entity.Modified = DateTime.UtcNow;
            entity.ModifiedBy = await currentUser;

            await Context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id, TUserKey currentUserId)
        {
            var currentUser = GetUserEntityByIdAsync(currentUserId);
            if (id == default(Guid) || id == Guid.Empty)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, $"{nameof(id)} is default or empty.");
            }

            var entity = await GetEntityByIdAsync(id);
            entity.IsDeleted = true;
            entity.Modified = DateTime.UtcNow;
            entity.ModifiedBy = await currentUser;

            await Context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            if (id == default(Guid) || id == Guid.Empty)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, $"{nameof(id)} is default or empty.");
            }

            var entity = await GetEntityByIdAsync(id);
            entity.IsDeleted = true;
            entity.Modified = DateTime.UtcNow;

            await Context.SaveChangesAsync();
        }
    }
}