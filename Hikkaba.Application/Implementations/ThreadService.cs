﻿using System.Net;
using Hikkaba.Application.Contracts;
using Hikkaba.Application.Telemetry.Metrics;
using Hikkaba.Infrastructure.Models.Ban.PostingRestrictions;
using Hikkaba.Infrastructure.Models.Error;
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
                 || postingRestrictionStatus is not PostingRestrictionsResponseSuccessModel noPostingRestrictionsModel)
        {
            return new DomainError
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                ErrorMessage = "Posting is restricted.",
            };
        }

        var threadSalt = Guid.NewGuid();
        var userIp = requestModel.UserIpAddress ?? [];
        var threadLocalUserHash = _hashService.GetHashBytes(threadSalt, userIp);
        var repoRequestModel = new ThreadCreateExtendedRequestModel
        {
            BaseModel = requestModel,
            ThreadSalt = threadSalt,
            ThreadLocalUserHash = threadLocalUserHash,
            ClientInfo = requestModel.ClientInfo,
        };

        await using var uploadedAttachments = await _attachmentService.UploadAttachmentsAsync(requestModel.BlobContainerId, attachments, cancellationToken);

        try
        {
            var createThreadResult = await _threadRepository.CreateThreadAsync(repoRequestModel, uploadedAttachments, cancellationToken);

            if (createThreadResult.Value is not ThreadPostCreateSuccessResultModel successResultModel)
            {
                return createThreadResult;
            }
            else
            {
                _threadMetrics.ThreadCreated(requestModel.CategoryAlias);
                _postMetrics.PostCreated(requestModel.CategoryAlias, uploadedAttachments.Count, uploadedAttachments.Sum(x => x.FileSize));

                foreach (var deletedBlobContainerId in successResultModel.DeletedBlobContainerIds)
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
                            successResultModel.PostId,
                            successResultModel.ThreadId);
                    }
                }

                return successResultModel;
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(
                LogEventIds.DeletingUploadedAttachments,
                e,
                "Failed to create post with attachments; deleting uploaded attachments within blob container {BlobContainerId}",
                requestModel.BlobContainerId);
            await _attachmentService.DeleteAttachmentsContainerAsync(requestModel.BlobContainerId);
            throw;
        }
    }

    public async Task<ThreadPatchResultModel> EditThreadAsync(ThreadEditRequestModel editRequestModel, CancellationToken cancellationToken)
    {
        return await _threadRepository.EditThreadAsync(editRequestModel, cancellationToken);
    }

    public async Task<ThreadPatchResultModel> SetThreadPinnedAsync(long threadId, bool isPinned, CancellationToken cancellationToken)
    {
        return await _threadRepository.SetThreadPinnedAsync(threadId, isPinned, cancellationToken);
    }

    public async Task<ThreadPatchResultModel> SetThreadCyclicAsync(long threadId, bool isCyclic, CancellationToken cancellationToken)
    {
        return await _threadRepository.SetThreadCyclicAsync(threadId, isCyclic, cancellationToken);
    }

    public async Task<ThreadPatchResultModel> SetThreadClosedAsync(long threadId, bool isClosed, CancellationToken cancellationToken)
    {
        return await _threadRepository.SetThreadClosedAsync(threadId, isClosed, cancellationToken);
    }

    public async Task<ThreadPatchResultModel> SetThreadDeletedAsync(long threadId, bool isDeleted, CancellationToken cancellationToken)
    {
        return await _threadRepository.SetThreadDeletedAsync(threadId, isDeleted, cancellationToken);
    }
}
