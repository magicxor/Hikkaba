using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Paging.Models;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Application.Contracts;

public interface IPostService
{
    Task<PostDetailsModel> GetPostAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<PostDetailsModel>> ListThreadPostsAsync(ThreadPostsFilter filter, CancellationToken cancellationToken);

    Task<PagedResult<PostDetailsModel>> SearchPostsPaginatedAsync(SearchPostsPagingFilter filter, CancellationToken cancellationToken);

    Task<PagedResult<PostDetailsModel>> ListPostsPaginatedAsync(PostPagingFilter filter, CancellationToken cancellationToken);

    Task<PostCreateResultModel> CreatePostAsync(PostCreateRequestModel requestModel, IFormFileCollection attachments, CancellationToken cancellationToken);

    Task EditPostAsync(PostEditRequestModel requestModel, CancellationToken cancellationToken);

    Task SetPostDeletedAsync(long postId, bool isDeleted, CancellationToken cancellationToken);
}
