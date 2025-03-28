using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Paging.Models;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Services.Contracts;

public interface IPostService
{
    Task<PostViewRm> GetPostAsync(long id);

    Task<IReadOnlyList<PostViewRm>> ListThreadPostsAsync(ThreadPostsFilter filter, CancellationToken cancellationToken);

    Task<PagedResult<PostViewRm>> SearchPostsPaginatedAsync(SearchPostsPagingFilter filter, CancellationToken cancellationToken);

    Task<PagedResult<PostViewRm>> ListPostsPaginatedAsync(PostPagingFilter filter, CancellationToken cancellationToken);

    Task<long> CreatePostAsync(PostCreateRm createRm, IFormFileCollection attachments, CancellationToken cancellationToken);

    Task EditPostAsync(PostEditRm editRm);

    Task SetPostDeletedAsync(long postId, bool isDeleted);
}
