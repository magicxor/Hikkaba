using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Hikkaba.Models.Dto.Thread;
using Hikkaba.Services.Implementations.Generic;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Services.Contracts;

public interface IThreadService
{
    Task<ThreadDto> GetAsync(TPrimaryKey id);
    Task<ThreadAggregationDto> GetAggregationAsync(TPrimaryKey id, ClaimsPrincipal user);

    Task<IList<ThreadDto>> ListAsync<TOrderKey>(
        Expression<Func<Thread, bool>> where = null,
        Expression<Func<Thread, TOrderKey>> orderBy = null,
        bool isDescending = false);

    Task<ThreadPostCreateResultDto> CreateThreadPostAsync(IFormFileCollection attachments, ThreadPostCreateDto dto, bool createNewThread);

    Task EditAsync(ThreadDto dto);

    Task<BasePagedList<ThreadDto>> PagedListAsync(Expression<Func<Thread, bool>> where, PageDto page = null);

    Task SetIsPinnedAsync(TPrimaryKey id, bool newValue);
    Task SetIsClosedAsync(TPrimaryKey id, bool newValue);
    Task SetIsDeletedAsync(TPrimaryKey id, bool newValue);
}