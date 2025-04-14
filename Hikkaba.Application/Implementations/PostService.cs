using System.Net;
using Hikkaba.Application.Contracts;
using Hikkaba.Application.Telemetry.Metrics;
using Hikkaba.Infrastructure.Models.Ban.PostingRestrictions;
using Hikkaba.Infrastructure.Models.Error;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Enums;
using Hikkaba.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Application.Implementations;

public sealed class PostService : IPostService
{
    private readonly ILogger<PostService> _logger;
    private readonly IAttachmentService _attachmentService;
    private readonly IPostRepository _postRepository;
    private readonly IBanRepository _banRepository;
    private readonly IHashService _hashService;
    private readonly PostMetrics _postMetrics;

    public PostService(
        ILogger<PostService> logger,
        IAttachmentService attachmentService,
        IPostRepository postRepository,
        IBanRepository banRepository,
        IHashService hashService,
        PostMetrics postMetrics)
    {
        _logger = logger;
        _attachmentService = attachmentService;
        _postRepository = postRepository;
        _banRepository = banRepository;
        _hashService = hashService;
        _postMetrics = postMetrics;
    }

    public async Task<PostDetailsModel?> GetPostAsync(long id, CancellationToken cancellationToken)
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

    public async Task<PostCreateResultModel> CreatePostAsync(
        PostCreateRequestModel requestModel,
        IFormFileCollection attachments,
        CancellationToken cancellationToken)
    {
        var postingRestrictionStatus = await _banRepository.GetPostingRestrictionStatusAsync(new PostingRestrictionsRequestModel
        {
            CategoryAlias = requestModel.CategoryAlias,
            ThreadId = requestModel.ThreadId,
            UserIpAddress = requestModel.UserIpAddress,
        }, cancellationToken);

        if (postingRestrictionStatus is PostingRestrictionsResponseFailureModel failurePostingRestrictionsModel)
        {
            return new DomainError
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                ErrorMessage = "Error processing request",
            };
        }
        else if (postingRestrictionStatus is PostingRestrictionsResponseBanModel banPostingRestrictionsModel)
        {
            return new DomainError
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                ErrorMessage = $"Posting is restricted. Reason: {banPostingRestrictionsModel.RestrictionReason}, expires: {banPostingRestrictionsModel.RestrictionEndsAt}",
            };
        }
        else if (postingRestrictionStatus.RestrictionType != PostingRestrictionType.NoRestriction
                 || postingRestrictionStatus is not PostingRestrictionsResponseSuccessModel noPostingRestrictionsModel
                 || noPostingRestrictionsModel.ThreadSalt is null)
        {
            return new DomainError
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                ErrorMessage = "Posting is restricted.",
            };
        }
        else
        {
            var threadSalt = noPostingRestrictionsModel.ThreadSalt.Value;
            var userIp = requestModel.UserIpAddress ?? [];
            var threadLocalUserHash = _hashService.GetHashBytes(threadSalt, userIp);
            var repoRequestModel = new PostCreateExtendedRequestModel
            {
                BaseModel = requestModel,
                ThreadLocalUserHash = threadLocalUserHash,
                IsCyclic = noPostingRestrictionsModel.IsCyclic,
                BumpLimit = noPostingRestrictionsModel.BumpLimit,
                PostCount = noPostingRestrictionsModel.PostCount,
            };

            await using var uploadedAttachments = await _attachmentService.UploadAttachmentsAsync(requestModel.BlobContainerId, attachments, cancellationToken);

            try
            {
                var createPostResult = await _postRepository.CreatePostAsync(repoRequestModel, uploadedAttachments, cancellationToken);

                _postMetrics.PostCreated(requestModel.CategoryAlias, uploadedAttachments.Count, uploadedAttachments.Sum(x => x.FileSize));

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
                _logger.LogWarning(e, "Failed to create post with attachments. Deleting uploaded attachments within blob container {BlobContainerId}", requestModel.BlobContainerId);
                await _attachmentService.DeleteAttachmentsContainerAsync(requestModel.BlobContainerId);
                throw;
            }
        }
    }

    public async Task EditPostAsync(PostEditRequestModel requestModel, CancellationToken cancellationToken)
    {
        await _postRepository.EditPostAsync(requestModel, cancellationToken);
    }

    public async Task SetPostDeletedAsync(long postId, bool isDeleted, CancellationToken cancellationToken)
    {
        await _postRepository.SetPostDeletedAsync(postId, isDeleted, cancellationToken);
    }
}
