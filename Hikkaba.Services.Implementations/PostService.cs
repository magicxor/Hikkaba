using System.Net;
using Hikkaba.Common.Enums;
using Hikkaba.Common.Exceptions;
using Hikkaba.Infrastructure.Models.Attachments;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Paging.Models;
using Hikkaba.Repositories.Contracts;
using Hikkaba.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Services.Implementations;

public class PostService : IPostService
{
    private readonly ILogger<PostService> _logger;
    private readonly IAttachmentService _attachmentService;
    private readonly IPostRepository _postRepository;
    private readonly IBanRepository _banRepository;

    public PostService(
        ILogger<PostService> logger,
        IAttachmentService attachmentService,
        IPostRepository postRepository,
        IBanRepository banRepository)
    {
        _logger = logger;
        _attachmentService = attachmentService;
        _postRepository = postRepository;
        _banRepository = banRepository;
    }

    public async Task<PostViewRm> GetPostAsync(long id)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<PostViewRm>> ListThreadPostsAsync(ThreadPostsFilter filter, CancellationToken cancellationToken)
    {
        return await _postRepository.ListThreadPostsAsync(filter, cancellationToken);
    }

    public async Task<PagedResult<PostViewRm>> SearchPostsPaginatedAsync(SearchPostsPagingFilter filter, CancellationToken cancellationToken)
    {
        return await _postRepository.SearchPostsPaginatedAsync(filter, cancellationToken);
    }

    public async Task<PagedResult<PostViewRm>> ListPostsPaginatedAsync(PostPagingFilter filter, CancellationToken cancellationToken)
    {
        return await _postRepository.ListPostsPaginatedAsync(filter, cancellationToken);
    }

    public async Task<long> CreatePostAsync(PostCreateRm createRm, IFormFileCollection attachments, CancellationToken cancellationToken)
    {
        var postingRestrictionStatus = await _banRepository.GetPostingRestrictionStatusAsync(new PostingRestrictionStatusRequestRm
        {
            ThreadId = createRm.ThreadId,
            UserIpAddress = createRm.UserIpAddress,
        });

        if (postingRestrictionStatus.RestrictionType != PostingRestrictionType.NoRestriction)
        {
            throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Posting is restricted: {postingRestrictionStatus.RestrictionType.ToString()}");
        }

        await using var uploadedAttachments = await _attachmentService.UploadAttachmentsAsync(createRm.BlobContainerId, attachments, cancellationToken);

        try
        {
            return await _postRepository.CreatePostAsync(createRm, uploadedAttachments, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to create post with attachments; deleting uploaded attachments");
            await _attachmentService.DeleteAttachmentsAsync(createRm.BlobContainerId);
            throw;
        }
    }

    public async Task EditPostAsync(PostEditRm editRm)
    {
        throw new NotImplementedException();
    }

    public async Task SetPostDeletedAsync(long postId, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}
