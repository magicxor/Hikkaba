using System.Net;
using Hikkaba.Application.Contracts;
using Hikkaba.Application.Telemetry.Metrics;
using Hikkaba.Infrastructure.Models.Ban.PostingRestrictions;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Enums;
using Hikkaba.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Application.Implementations;

public sealed class ThreadService : IThreadService
{
    private readonly ILogger<ThreadService> _logger;
    private readonly IAttachmentService _attachmentService;
    private readonly IThreadRepository _threadRepository;
    private readonly IBanRepository _banRepository;
    private readonly IHashService _hashService;
    private readonly ThreadMetrics _threadMetrics;
    private readonly PostMetrics _postMetrics;

    public ThreadService(
        ILogger<ThreadService> logger,
        IAttachmentService attachmentService,
        IThreadRepository threadRepository,
        IBanRepository banRepository,
        IHashService hashService,
        ThreadMetrics threadMetrics,
        PostMetrics postMetrics)
    {
        _logger = logger;
        _attachmentService = attachmentService;
        _threadRepository = threadRepository;
        _banRepository = banRepository;
        _hashService = hashService;
        _threadMetrics = threadMetrics;
        _postMetrics = postMetrics;
    }

    public async Task<CategoryThreadModel?> GetCategoryThreadAsync(CategoryThreadFilter filter, CancellationToken cancellationToken)
    {
        return await _threadRepository.GetCategoryThreadAsync(filter, cancellationToken);
    }

    public async Task<ThreadDetailsRequestModel?> GetThreadDetailsAsync(long threadId, CancellationToken cancellationToken)
    {
        return await _threadRepository.GetThreadDetailsAsync(threadId, false, cancellationToken);
    }

    public async Task<PagedResult<ThreadPreviewModel>> ListThreadPreviewsPaginatedAsync(ThreadPreviewFilter filter, CancellationToken cancellationToken)
    {
        return await _threadRepository.ListThreadPreviewsPaginatedAsync(filter, cancellationToken);
    }

    public async Task<ThreadPostCreateResultModel> CreateThreadAsync(
        ThreadCreateRequestModel requestModel,
        IFormFileCollection attachments,
        CancellationToken cancellationToken)
    {
        var postingRestrictionStatus = await _banRepository.GetPostingRestrictionStatusAsync(new PostingRestrictionsRequestModel
        {
            CategoryAlias = requestModel.CategoryAlias,
            ThreadId = null,
            UserIpAddress = requestModel.UserIpAddress,
        }, cancellationToken);

        if (postingRestrictionStatus.RestrictionType != PostingRestrictionType.NoRestriction
            || postingRestrictionStatus is not PostingRestrictionsResponseSuccessModel successModel)
        {
            throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Posting is restricted: {Enum.GetName(postingRestrictionStatus.RestrictionType)}");
        }

        var threadSalt = Guid.NewGuid();
        var userIp = requestModel.UserIpAddress ?? [];
        var threadLocalUserHash = _hashService.GetHashBytes(threadSalt, userIp);
        var repoRequestModel = new ThreadCreateExtendedRequestModel
        {
            BaseModel = requestModel,
            ThreadSalt = threadSalt,
            ThreadLocalUserHash = threadLocalUserHash,
        };

        await using var uploadedAttachments = await _attachmentService.UploadAttachmentsAsync(requestModel.BlobContainerId, attachments, cancellationToken);

        try
        {
            var createThreadResult = await _threadRepository.CreateThreadAsync(repoRequestModel, uploadedAttachments, cancellationToken);

            _threadMetrics.ThreadCreated(requestModel.CategoryAlias);
            _postMetrics.PostCreated(requestModel.CategoryAlias, uploadedAttachments.Count, uploadedAttachments.Sum(x => x.FileSize));

            foreach (var deletedBlobContainerId in createThreadResult.DeletedBlobContainerIds)
            {
                try
                {
                    await _attachmentService.DeleteAttachmentsContainerAsync(deletedBlobContainerId);
                }
                catch (Exception deleteBlobContainerException)
                {
                    _logger.LogError(deleteBlobContainerException,
                        "Failed to delete blob container {BlobContainerId} after thread creation. Post Id: {PostId}, Thread Id: {ThreadId}",
                        deletedBlobContainerId,
                        createThreadResult.PostId,
                        createThreadResult.ThreadId);
                }
            }

            return createThreadResult;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to create post with attachments; deleting uploaded attachments");
            await _attachmentService.DeleteAttachmentsContainerAsync(requestModel.BlobContainerId);
            throw;
        }
    }

    public async Task EditThreadAsync(ThreadEditRequestModel requestModel, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task SetIsPinnedAsync(long threadId, bool isPinned, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task SetIsClosedAsync(long threadId, bool isClosed, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task SetIsDeletedAsync(long threadId, bool isDeleted, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
