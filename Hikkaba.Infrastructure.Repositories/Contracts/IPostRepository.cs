using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IPostRepository
{
    Task<IReadOnlyList<PostDetailsModel>> ListThreadPostsAsync(ThreadPostsFilter filter, CancellationToken cancellationToken);

    Task<PagedResult<PostDetailsModel>> SearchPostsPaginatedAsync(SearchPostsPagingFilter filter, CancellationToken cancellationToken);

    Task<PagedResult<PostDetailsModel>> ListPostsPaginatedAsync(PostPagingFilter filter, CancellationToken cancellationToken);

    Task<long> CreatePostAsync(PostCreateRequestModel createRequestModel, byte[] threadLocalUserHash, FileAttachmentContainerCollection inputFiles, CancellationToken cancellationToken);
}
