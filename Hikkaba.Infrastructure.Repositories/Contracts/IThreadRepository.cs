using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IThreadRepository
{
    Task<CategoryThreadModel?> GetCategoryThreadAsync(CategoryThreadFilter filter, CancellationToken cancellationToken);

    Task<ThreadDetailsRequestModel?> GetThreadDetailsAsync(long threadId, bool includeDeleted, CancellationToken cancellationToken);

    Task<PagedResult<ThreadPreviewModel>> ListThreadPreviewsPaginatedAsync(ThreadPreviewFilter filter, CancellationToken cancellationToken);

    Task<ThreadPostCreateResultModel> CreateThreadAsync(ThreadCreateExtendedRequestModel createRequestModel, FileAttachmentContainerCollection inputFiles, CancellationToken cancellationToken);

    Task<ThreadPatchResultModel> SetThreadPinnedAsync(long threadId, bool isPinned, CancellationToken cancellationToken);

    Task<ThreadPatchResultModel> SetThreadCyclicAsync(long threadId, bool isCyclic, CancellationToken cancellationToken);

    Task<ThreadPatchResultModel> SetThreadClosedAsync(long threadId, bool isClosed, CancellationToken cancellationToken);

    Task<ThreadPatchResultModel> SetThreadDeletedAsync(long threadId, bool isDeleted, CancellationToken cancellationToken);
}
