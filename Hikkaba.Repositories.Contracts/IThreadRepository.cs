using Hikkaba.Infrastructure.Models.Attachments;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Paging.Models;

namespace Hikkaba.Repositories.Contracts;

public interface IThreadRepository
{
    Task<ThreadDetailsRm?> GetThreadDetailsAsync(long threadId, bool includeDeleted, CancellationToken cancellationToken);

    Task<IReadOnlyList<long>> ListAllThreadIdsAsync(CancellationToken cancellationToken);

    Task<PagedResult<ThreadPreviewRm>> ListThreadPreviewsPaginatedAsync(ThreadPreviewsFilter filter, CancellationToken cancellationToken);

    Task<ThreadPostCreateResultRm> CreateThreadAsync(ThreadCreateRm createRm, FileAttachmentCollection inputFiles, CancellationToken cancellationToken);
}
