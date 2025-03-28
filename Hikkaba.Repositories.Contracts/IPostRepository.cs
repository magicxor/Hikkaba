using Hikkaba.Infrastructure.Models.Attachments;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Paging.Models;

namespace Hikkaba.Repositories.Contracts;

public interface IPostRepository
{
    Task<IReadOnlyList<PostViewRm>> ListThreadPostsAsync(ThreadPostsFilter filter, CancellationToken cancellationToken);

    Task<PagedResult<PostViewRm>> SearchPostsPaginatedAsync(SearchPostsPagingFilter filter, CancellationToken cancellationToken);

    Task<PagedResult<PostViewRm>> ListPostsPaginatedAsync(PostPagingFilter filter, CancellationToken cancellationToken);

    Task<long> CreatePostAsync(PostCreateRm createRm, FileAttachmentCollection inputFiles, CancellationToken cancellationToken);
}
