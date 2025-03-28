using System.Net;
using Hikkaba.Common.Enums;
using Hikkaba.Common.Exceptions;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Paging.Models;
using Hikkaba.Repositories.Contracts;
using Hikkaba.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Services.Implementations;

public class ThreadService : IThreadService
{
    private readonly ILogger<ThreadService> _logger;
    private readonly IAttachmentService _attachmentService;
    private readonly IThreadRepository _threadRepository;
    private readonly IBanRepository _banRepository;

    public ThreadService(
        ILogger<ThreadService> logger,
        IAttachmentService attachmentService,
        IThreadRepository threadRepository,
        IBanRepository banRepository)
    {
        _logger = logger;
        _attachmentService = attachmentService;
        _threadRepository = threadRepository;
        _banRepository = banRepository;
    }

    public async Task<ThreadEditRm> GetThreadAsync(long threadId)
    {
        throw new NotImplementedException();
    }

    public async Task<ThreadDetailsRm?> GetThreadDetailsAsync(long threadId, CancellationToken cancellationToken)
    {
        return await _threadRepository.GetThreadDetailsAsync(threadId, false, cancellationToken);
    }

    public async Task<IReadOnlyList<long>> ListAllThreadIdsAsync(CancellationToken cancellationToken)
    {
        return await _threadRepository.ListAllThreadIdsAsync(cancellationToken);
    }

    public async Task<PagedResult<ThreadPreviewRm>> ListThreadPreviewsPaginatedAsync(ThreadPreviewsFilter filter, CancellationToken cancellationToken)
    {
        return await _threadRepository.ListThreadPreviewsPaginatedAsync(filter, cancellationToken);
    }

    public async Task<ThreadPostCreateResultRm> CreateThreadAsync(ThreadCreateRm createRm, IFormFileCollection attachments, CancellationToken cancellationToken)
    {
        var postingRestrictionStatus = await _banRepository.GetPostingRestrictionStatusAsync(new PostingRestrictionStatusRequestRm
        {
            CategoryAlias = createRm.CategoryAlias,
            UserIpAddress = createRm.UserIpAddress,
        });

        if (postingRestrictionStatus.RestrictionType != PostingRestrictionType.NoRestriction)
        {
            throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Posting is restricted: {postingRestrictionStatus.RestrictionType.ToString()}");
        }

        await using var uploadedAttachments = await _attachmentService.UploadAttachmentsAsync(createRm.BlobContainerId, attachments, cancellationToken);

        try
        {
            return await _threadRepository.CreateThreadAsync(createRm, uploadedAttachments, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to create post with attachments; deleting uploaded attachments");
            await _attachmentService.DeleteAttachmentsAsync(createRm.BlobContainerId);
            throw;
        }
    }

    public async Task EditThreadAsync(ThreadEditRm editRm)
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
