using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Paging.Models;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Application.Contracts;

public interface IPostService
{
    Task<PostDetailsModel> GetPostAsync(long id);

    Task<IReadOnlyList<PostDetailsModel>> ListThreadPostsAsync(ThreadPostsFilter filter, CancellationToken cancellationToken);

    Task<PagedResult<PostDetailsModel>> SearchPostsPaginatedAsync(SearchPostsPagingFilter filter, CancellationToken cancellationToken);

    Task<PagedResult<PostDetailsModel>> ListPostsPaginatedAsync(PostPagingFilter filter, CancellationToken cancellationToken);

    Task<long> CreatePostAsync(PostCreateRequestModel createRequestModel, IFormFileCollection attachments, CancellationToken cancellationToken);

    Task EditPostAsync(PostEditRequestModel editRequestModel);

    Task SetPostDeletedAsync(long postId, bool isDeleted);
}
