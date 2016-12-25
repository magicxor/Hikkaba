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
    public interface IBaseModeratedMutableEntityService<TDto, TEntity, TUserKey> : IBaseMutableEntityService<TDto, TEntity, TUserKey>
    {
    }

    public abstract class BaseModeratedMutableEntityService<TDto, TEntity, TUserKey> :
        BaseMutableEntityService<TDto, TEntity, TUserKey>, 
        IBaseModeratedMutableEntityService<TDto, TEntity, TUserKey>
        where TDto : BaseMutableDto
        where TEntity : BaseMutableEntity
    {
        private readonly UserManager<ApplicationUser> _userManager;
        
        public BaseModeratedMutableEntityService(IMapper mapper, 
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager) : base(mapper, context)
        {
            _userManager = userManager;
        }

        protected abstract Guid GetCategoryId(TEntity entity);
        protected abstract IBaseManyToManyService<Guid, TUserKey> GetManyToManyService();

        protected async Task<bool> HasPermissionToEdit(Guid entityId, TUserKey currentUserId)
        {
            var currentUser = await GetUserEntityByIdAsync(currentUserId);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, Defaults.DefaultAdminRoleName);
            if (isAdmin)
            {
                return true;
            }
            else
            {
                var isCategoryModerator = false;
                var requestedEntity = await GetDbSetWithReferences(Context).FirstOrDefaultAsync(entity => entity.Id == entityId);
                if (requestedEntity != null)
                {
                    var categoryId = GetCategoryId(requestedEntity);
                    var manyToManyService = GetManyToManyService();
                    isCategoryModerator = await manyToManyService.AreRelatedAsync(categoryId, currentUserId);
                }
                return isCategoryModerator;
            }
        }

        public override async Task EditAsync(TDto dto, TUserKey currentUserId)
        {
            if (dto == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, $"{nameof(dto)} is null");
            }
            else
            {
                var hasPermissionToEdit = await HasPermissionToEdit(dto.Id, currentUserId);
                if (hasPermissionToEdit)
                {
                    await base.EditAsync(dto, currentUserId);
                }
                else
                {
                    throw new HttpResponseException(HttpStatusCode.Forbidden, $"You don't have permission to edit {dto.Id}");
                }
            }
        }

        public override async Task DeleteAsync(Guid entityId, TUserKey currentUserId)
        {
            var hasPermissionToEdit = await HasPermissionToEdit(entityId, currentUserId);
            if (hasPermissionToEdit)
            {
                await base.DeleteAsync(entityId, currentUserId);
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden, $"You don't have permission to edit {entityId}");
            }
        }
    }
}
