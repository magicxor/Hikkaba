using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Paging.Models;
using Hikkaba.Repositories.Contracts;
using Hikkaba.Services.Contracts;

namespace Hikkaba.Services.Implementations;

public class ThreadService : IThreadService
{
    private readonly IThreadRepository _threadRepository;

    public ThreadService(IThreadRepository threadRepository)
    {
        _threadRepository = threadRepository;
    }

    public async Task<ThreadEditSm> GetAsync(long threadId)
    {
        throw new NotImplementedException();
    }

    public async Task<ThreadAggregationSm> GetThreadDetailsAsync(long threadId)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<long>> ListAllThreadIdsAsync(CancellationToken cancellationToken)
    {
        return await _threadRepository.ListAllThreadIdsAsync(cancellationToken);
    }

    public async Task<PagedResult<ThreadPreviewSm>> ListThreadPreviewsPaginatedAsync(ThreadPreviewsFilter filter, CancellationToken cancellationToken)
    {
        return await _threadRepository.ListThreadPreviewsPaginatedAsync(filter, cancellationToken);
    }

    public async Task EditAsync(ThreadEditSm editSm)
    {
        throw new NotImplementedException();
    }

    public async Task SetIsPinnedAsync(long threadId, bool isPinned)
    {
        throw new NotImplementedException();
    }

    public async Task SetIsClosedAsync(long threadId, bool isClosed)
    {
        throw new NotImplementedException();
    }

    public async Task SetIsDeletedAsync(long threadId, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}
