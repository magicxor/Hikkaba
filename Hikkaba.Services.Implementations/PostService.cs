using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Paging.Models;
using Hikkaba.Repositories.Contracts;
using Hikkaba.Services.Contracts;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Services.Implementations;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;

    public PostService(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<PostDto> GetPostAsync(long id)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<PostInfoRm>> ListThreadPostsAsync(ThreadPostsFilter filter, CancellationToken cancellationToken)
    {
        return await _postRepository.ListThreadPostsAsync(filter, cancellationToken);
    }

    public async Task<PagedResult<PostInfoRm>> SearchPostsPaginatedAsync(SearchPostsPagingFilter filter, CancellationToken cancellationToken)
    {
        return await _postRepository.SearchPostsPaginatedAsync(filter, cancellationToken);
    }

    public async Task<PagedResult<PostInfoRm>> ListPostsPaginatedAsync(PostPagingFilter filter, CancellationToken cancellationToken)
    {
        return await _postRepository.ListPostsPaginatedAsync(filter, cancellationToken);
    }

    public async Task<ThreadPostCreateResultSm> CreatePostAsync(IFormFileCollection attachments, ThreadPostCreateSm createSm, bool startNewThread)
    {
        throw new NotImplementedException();
    }

    public async Task EditPostAsync(PostDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task SetPostDeletedAsync(long postId, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}
