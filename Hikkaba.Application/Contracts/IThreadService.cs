using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Paging.Models;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Application.Contracts;

public interface IThreadService
{
    Task<CategoryThreadModel?> GetCategoryThreadAsync(CategoryThreadFilter filter, CancellationToken cancellationToken);

    Task<ThreadDetailsRequestModel?> GetThreadDetailsAsync(long threadId, CancellationToken cancellationToken);

    Task<PagedResult<ThreadPreviewModel>> ListThreadPreviewsPaginatedAsync(ThreadPreviewFilter filter, CancellationToken cancellationToken);

    Task<ThreadPostCreateResultModel> CreateThreadAsync(ThreadCreateRequestModel requestModel, IFormFileCollection attachments, CancellationToken cancellationToken);

    Task EditThreadAsync(ThreadEditRequestModel requestModel, CancellationToken cancellationToken);

    Task SetIsPinnedAsync(long threadId, bool isPinned, CancellationToken cancellationToken);

    Task SetIsClosedAsync(long threadId, bool isClosed, CancellationToken cancellationToken);

    Task SetIsDeletedAsync(long threadId, bool isDeleted, CancellationToken cancellationToken);
}
