using System.Net;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Models.Ban.PostingRestrictions;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Enums;
using Hikkaba.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Application.Implementations;

public class ThreadService : IThreadService
{
    private readonly ILogger<ThreadService> _logger;
    private readonly IAttachmentService _attachmentService;
    private readonly IThreadRepository _threadRepository;
    private readonly IBanRepository _banRepository;
    private readonly IHashService _hashService;

    public ThreadService(
        ILogger<ThreadService> logger,
        IAttachmentService attachmentService,
        IThreadRepository threadRepository,
        IBanRepository banRepository,
        IHashService hashService)
    {
        _logger = logger;
        _attachmentService = attachmentService;
        _threadRepository = threadRepository;
        _banRepository = banRepository;
        _hashService = hashService;
    }

    public async Task<ThreadEditRequestModel> GetThreadAsync(long threadId)
    {
        throw new NotImplementedException();
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
        ThreadCreateRequestModel createRequestModel,
        IFormFileCollection attachments,
        CancellationToken cancellationToken)
    {
        var postingRestrictionStatus = await _banRepository.GetPostingRestrictionStatusAsync(new PostingRestrictionsRequestModel
        {
            CategoryAlias = createRequestModel.CategoryAlias,
            ThreadId = null,
            UserIpAddress = createRequestModel.UserIpAddress,
        }, cancellationToken);

        if (postingRestrictionStatus.RestrictionType != PostingRestrictionType.NoRestriction
            || postingRestrictionStatus is not PostingRestrictionsResponseSuccessModel successModel)
        {
            throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Posting is restricted: {Enum.GetName(postingRestrictionStatus.RestrictionType)}");
        }

        var threadSalt = Guid.NewGuid();
        var userIp = createRequestModel.UserIpAddress ?? [];
        var threadLocalUserHash = _hashService.GetHashBytes(threadSalt, userIp);
        var repoRequestModel = new ThreadCreateExtendedRequestModel
        {
            BaseModel = createRequestModel,
            ThreadSalt = threadSalt,
            ThreadLocalUserHash = threadLocalUserHash,
        };

        await using var uploadedAttachments = await _attachmentService.UploadAttachmentsAsync(createRequestModel.BlobContainerId, attachments, cancellationToken);

        try
        {
            var createThreadResult = await _threadRepository.CreateThreadAsync(repoRequestModel, uploadedAttachments, cancellationToken);

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
            await _attachmentService.DeleteAttachmentsContainerAsync(createRequestModel.BlobContainerId);
            throw;
        }
    }

    public async Task EditThreadAsync(ThreadEditRequestModel editRequestModel)
    {
        throw new NotImplementedException();
    }

    public async Task SetIsPinnedAsync(long threadId, bool isPinned)
    {
        throw new NotImplementedException();
    }

    public async Task SetIsClosedAsync(long threadId, bool isClosed)
    {
        throw new NotImplementedException();
    }

    public async Task SetIsDeletedAsync(long threadId, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}
