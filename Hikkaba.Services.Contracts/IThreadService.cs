using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Paging.Models;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Services.Contracts;

public interface IThreadService
{
    Task<ThreadEditRm> GetThreadAsync(long threadId);

    Task<ThreadDetailsRm?> GetThreadDetailsAsync(long threadId, CancellationToken cancellationToken);

    Task<IReadOnlyList<long>> ListAllThreadIdsAsync(CancellationToken cancellationToken);

    Task<PagedResult<ThreadPreviewRm>> ListThreadPreviewsPaginatedAsync(ThreadPreviewsFilter filter, CancellationToken cancellationToken);

    Task<ThreadPostCreateResultRm> CreateThreadAsync(ThreadCreateRm createRm, IFormFileCollection attachments, CancellationToken cancellationToken);

    Task EditThreadAsync(ThreadEditRm editRm);

    Task SetIsPinnedAsync(long threadId, bool isPinned);

    Task SetIsClosedAsync(long threadId, bool isClosed);

    Task SetIsDeletedAsync(long threadId, bool isDeleted);
}
