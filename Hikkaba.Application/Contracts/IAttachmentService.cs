using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Application.Contracts;

public interface IAttachmentService
{
    Task<FileAttachmentContainerCollection> UploadAttachmentsAsync(
        Guid blobContainerId,
        IFormFileCollection formFileCollection,
        CancellationToken cancellationToken);

    Task DeleteAttachmentAsync(Guid blobContainerId, Guid blobId);

    Task DeleteAttachmentsContainerAsync(Guid blobContainerId);
}
