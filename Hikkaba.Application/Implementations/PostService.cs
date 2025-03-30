using System.Net;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Enums;
using Hikkaba.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Application.Implementations;

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

    public async Task<PostDetailsModel> GetPostAsync(long id)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<PostDetailsModel>> ListThreadPostsAsync(ThreadPostsFilter filter, CancellationToken cancellationToken)
    {
        return await _postRepository.ListThreadPostsAsync(filter, cancellationToken);
    }

    public async Task<PagedResult<PostDetailsModel>> SearchPostsPaginatedAsync(SearchPostsPagingFilter filter, CancellationToken cancellationToken)
    {
        return await _postRepository.SearchPostsPaginatedAsync(filter, cancellationToken);
    }

    public async Task<PagedResult<PostDetailsModel>> ListPostsPaginatedAsync(PostPagingFilter filter, CancellationToken cancellationToken)
    {
        return await _postRepository.ListPostsPaginatedAsync(filter, cancellationToken);
    }

    public async Task<long> CreatePostAsync(PostCreateRequestModel createRequestModel, IFormFileCollection attachments, CancellationToken cancellationToken)
    {
        var postingRestrictionStatus = await _banRepository.GetPostingRestrictionStatusAsync(new PostingRestrictionsRequestModel
        {
            ThreadId = createRequestModel.ThreadId,
            UserIpAddress = createRequestModel.UserIpAddress,
        });

        if (postingRestrictionStatus.RestrictionType != PostingRestrictionType.NoRestriction)
        {
            throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Posting is restricted: {postingRestrictionStatus.RestrictionType.ToString()}");
        }

        await using var uploadedAttachments = await _attachmentService.UploadAttachmentsAsync(createRequestModel.BlobContainerId, attachments, cancellationToken);

        try
        {
            return await _postRepository.CreatePostAsync(createRequestModel, uploadedAttachments, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to create post with attachments; deleting uploaded attachments");
            await _attachmentService.DeleteAttachmentsAsync(createRequestModel.BlobContainerId);
            throw;
        }
    }

    public async Task EditPostAsync(PostEditRequestModel editRequestModel)
    {
        throw new NotImplementedException();
    }

    public async Task SetPostDeletedAsync(long postId, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}
