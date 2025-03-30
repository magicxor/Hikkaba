using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Paging.Models;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Application.Contracts;

public interface IThreadService
{
    Task<ThreadEditRequestModel> GetThreadAsync(long threadId);

    Task<ThreadDetailsRequestModel?> GetThreadDetailsAsync(long threadId, CancellationToken cancellationToken);

    Task<IReadOnlyList<long>> ListAllThreadIdsAsync(CancellationToken cancellationToken);

    Task<PagedResult<ThreadPreviewModel>> ListThreadPreviewsPaginatedAsync(ThreadPreviewFilter filter, CancellationToken cancellationToken);

    Task<ThreadPostCreateResultModel> CreateThreadAsync(ThreadCreateRequestModel createRequestModel, IFormFileCollection attachments, CancellationToken cancellationToken);

    Task EditThreadAsync(ThreadEditRequestModel editRequestModel);

    Task SetIsPinnedAsync(long threadId, bool isPinned);

    Task SetIsClosedAsync(long threadId, bool isClosed);

    Task SetIsDeletedAsync(long threadId, bool isDeleted);
}
