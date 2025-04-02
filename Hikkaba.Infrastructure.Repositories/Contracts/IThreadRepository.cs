using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IThreadRepository
{
    Task<ThreadDetailsRequestModel?> GetThreadDetailsAsync(long threadId, bool includeDeleted, CancellationToken cancellationToken);

    Task<PagedResult<ThreadPreviewModel>> ListThreadPreviewsPaginatedAsync(ThreadPreviewFilter filter, CancellationToken cancellationToken);

    Task<ThreadPostCreateResultModel> CreateThreadAsync(ThreadCreateRequestModel createRequestModel, Guid threadSalt, byte[] threadLocalUserHash, FileAttachmentContainerCollection inputFiles, CancellationToken cancellationToken);
}
