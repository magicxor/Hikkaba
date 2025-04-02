using System.Net;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Models.Ban.PostingRestrictions;
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
    private readonly IHashService _hashService;

    public PostService(
        ILogger<PostService> logger,
        IAttachmentService attachmentService,
        IPostRepository postRepository,
        IBanRepository banRepository,
        IHashService hashService)
    {
        _logger = logger;
        _attachmentService = attachmentService;
        _postRepository = postRepository;
        _banRepository = banRepository;
        _hashService = hashService;
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

    public async Task<long> CreatePostAsync(
        PostCreateRequestModel createRequestModel,
        IFormFileCollection attachments,
        CancellationToken cancellationToken)
    {
        var postingRestrictionStatus = await _banRepository.GetPostingRestrictionStatusAsync(new PostingRestrictionsRequestModel
        {
            CategoryAlias = createRequestModel.CategoryAlias,
            ThreadId = createRequestModel.ThreadId,
            UserIpAddress = createRequestModel.UserIpAddress,
        }, cancellationToken);

        if (postingRestrictionStatus.RestrictionType != PostingRestrictionType.NoRestriction
            || postingRestrictionStatus is not PostingRestrictionsResponseSuccessModel successModel
            || successModel.ThreadSalt is null)
        {
            throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Posting is restricted: {Enum.GetName(postingRestrictionStatus.RestrictionType)}");
        }

        var threadSalt = successModel.ThreadSalt.Value;
        var userIp = createRequestModel.UserIpAddress ?? [];
        var threadLocalUserHash = _hashService.GetHashBytes(threadSalt, userIp);
        var repoRequestModel = new PostCreateExtendedRequestModel
        {
            BaseModel = createRequestModel,
            ThreadLocalUserHash = threadLocalUserHash,
            IsCyclic = successModel.IsCyclic,
            BumpLimit = successModel.BumpLimit,
            PostCount = successModel.PostCount,
        };

        await using var uploadedAttachments = await _attachmentService.UploadAttachmentsAsync(createRequestModel.BlobContainerId, attachments, cancellationToken);

        try
        {
            var createPostResult = await _postRepository.CreatePostAsync(repoRequestModel, uploadedAttachments, cancellationToken);

            foreach (var deletedBlobContainerId in createPostResult.DeletedBlobContainerIds)
            {
                try
                {
                    await _attachmentService.DeleteAttachmentsContainerAsync(deletedBlobContainerId);
                }
                catch (Exception deleteBlobContainerException)
                {
                    _logger.LogError(deleteBlobContainerException, "Failed to delete blob container {BlobContainerId} after post creation. Post Id: {PostId}", deletedBlobContainerId, createPostResult.PostId);
                }
            }

            return createPostResult.PostId;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to create post with attachments. Deleting uploaded attachments within blob container {BlobContainerId}", createRequestModel.BlobContainerId);
            await _attachmentService.DeleteAttachmentsContainerAsync(createRequestModel.BlobContainerId);
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
