using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Paging.Models;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Services.Contracts;

public interface IThreadService
{
    Task<ThreadEditSm> GetAsync(long threadId);

    Task<ThreadAggregationSm> GetThreadDetailsAsync(long threadId);

    Task<IReadOnlyList<long>> ListAllThreadIdsAsync(CancellationToken cancellationToken);

    Task<PagedResult<ThreadPreviewSm>> ListThreadPreviewsPaginatedAsync(ThreadPreviewsFilter filter, CancellationToken cancellationToken);

    Task EditAsync(ThreadEditSm editSm);

    Task SetIsPinnedAsync(long threadId, bool isPinned);

    Task SetIsClosedAsync(long threadId, bool isClosed);

    Task SetIsDeletedAsync(long threadId, bool isDeleted);
}
