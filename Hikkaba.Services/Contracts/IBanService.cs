using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Hikkaba.Models.Dto.Ban;
using Hikkaba.Services.Implementations.Generic;

namespace Hikkaba.Services.Contracts;

public interface IBanService
{
    Task<BanDto> GetAsync(TPrimaryKey id);

    Task<IList<BanDto>> ListAsync<TOrderKey>(
        Expression<Func<Ban, bool>> where = null,
        Expression<Func<Ban, TOrderKey>> orderBy = null,
        bool isDescending = false);

    Task<BasePagedList<BanDto>> PagedListAsync<TOrderKey>(
        Expression<Func<Ban, bool>> where = null,
        Expression<Func<Ban, TOrderKey>> orderBy = null, bool isDescending = false,
        PageDto page = null);

    Task<PostingPermissionDto> IsPostingAllowedAsync(TPrimaryKey threadId, string userIpAddress);

    Task<TPrimaryKey> CreateAsync(BanEditDto dto);

    Task EditAsync(BanEditDto dto);

    Task SetIsDeletedAsync(TPrimaryKey id, bool newValue);
}