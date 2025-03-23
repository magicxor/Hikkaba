using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.ApplicationUser;
using Hikkaba.Paging.Models;

namespace Hikkaba.Services.Contracts;

public interface IApplicationUserService
{
    Task<ApplicationUserDto> GetAsync(int id);

    Task<IReadOnlyList<ApplicationUserDto>> ListAsync(ApplicationUserFilter filter);

    Task<int> CreateAsync(ApplicationUserDto dto);

    Task EditAsync(ApplicationUserDto dto);

    Task SetIsDeletedAsync(int id, bool newValue);
}
