using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Paging.Models;

namespace Hikkaba.Repositories.Contracts;

public interface IPostRepository
{
    Task<IReadOnlyList<PostInfoRm>> ListThreadPostsAsync(ThreadPostsFilter filter, CancellationToken cancellationToken);

    Task<PagedResult<PostInfoRm>> SearchPostsPaginatedAsync(SearchPostsPagingFilter filter, CancellationToken cancellationToken);

    Task<PagedResult<PostInfoRm>> ListPostsPaginatedAsync(PostPagingFilter filter, CancellationToken cancellationToken);
}
