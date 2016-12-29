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
    public interface IBaseModeratedMutableEntityService<TDto, TEntity, TPrimaryKey> : IBaseMutableEntityService<TDto, TEntity, TPrimaryKey>
    {
    }

    public abstract class BaseModeratedMutableEntityService<TDto, TEntity, TPrimaryKey> :
        BaseMutableEntityService<TDto, TEntity, TPrimaryKey>, 
        IBaseModeratedMutableEntityService<TDto, TEntity, TPrimaryKey>
        where TDto : class, IBaseMutableDto<TPrimaryKey>
        where TEntity : class, IBaseMutableEntity<TPrimaryKey>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        
        public BaseModeratedMutableEntityService(IMapper mapper, 
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager) : base(mapper, context)
        {
            _userManager = userManager;
        }

        protected abstract TPrimaryKey GetCategoryId(TEntity entity);
        protected abstract IBaseManyToManyService<TPrimaryKey, TPrimaryKey> GetManyToManyService();

        protected async Task<bool> HasPermissionToEdit(TPrimaryKey entityId, TPrimaryKey currentUserId)
        {
            var currentUser = await GetUserEntityByIdAsync(currentUserId);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, Defaults.AdministratorRoleName);
            if (isAdmin)
            {
                return true;
            }
            else
            {
                var isCategoryModerator = false;
                var requestedEntity = await GetDbSetWithReferences(Context).FirstOrDefaultAsync(entity => entity.Id.Equals(entityId));
                if (requestedEntity != null)
                {
                    var categoryId = GetCategoryId(requestedEntity);
                    var manyToManyService = GetManyToManyService();
                    isCategoryModerator = await manyToManyService.AreRelatedAsync(categoryId, currentUserId);
                }
                return isCategoryModerator;
            }
        }

        public override async Task EditAsync(TDto dto, TPrimaryKey currentUserId, Action<TEntity> setForeignKeys)
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
                    await base.EditAsync(dto, currentUserId, setForeignKeys);
                }
                else
                {
                    throw new HttpResponseException(HttpStatusCode.Forbidden, $"You don't have permission to edit {dto.Id}");
                }
            }
        }

        public override async Task DeleteAsync(TPrimaryKey entityId, TPrimaryKey currentUserId)
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
