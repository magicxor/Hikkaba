using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Hikkaba.Services.Implementations.Generic;

namespace Hikkaba.Services.Contracts;

public interface IApplicationUserService
{
    Task<ApplicationUserDto> GetAsync(TPrimaryKey id);

    Task<IList<ApplicationUserDto>> ListAsync<TOrderKey>(
        Expression<Func<ApplicationUser, bool>> where = null,
        Expression<Func<ApplicationUser, TOrderKey>> orderBy = null,
        bool isDescending = false);

    Task<BasePagedList<ApplicationUserDto>> PagedListAsync<TOrderKey>(
        Expression<Func<ApplicationUser, bool>> where = null,
        Expression<Func<ApplicationUser, TOrderKey>> orderBy = null, bool isDescending = false,
        PageDto page = null);

    Task<TPrimaryKey> CreateAsync(ApplicationUserDto dto);

    Task EditAsync(ApplicationUserDto dto);

    Task SetIsDeletedAsync(TPrimaryKey id, bool newValue);
}