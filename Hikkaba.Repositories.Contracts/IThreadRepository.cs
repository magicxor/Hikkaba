using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Paging.Models;

namespace Hikkaba.Repositories.Contracts;

public interface IThreadRepository
{
    Task<IReadOnlyList<long>> ListAllThreadIdsAsync(CancellationToken cancellationToken);

    Task<PagedResult<ThreadPreviewSm>> ListThreadPreviewsPaginatedAsync(ThreadPreviewsFilter filter, CancellationToken cancellationToken);
}
