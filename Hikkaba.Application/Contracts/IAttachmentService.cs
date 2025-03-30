using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Application.Contracts;

public interface IAttachmentService
{
    Task<FileAttachmentContainerCollection> UploadAttachmentsAsync(
        Guid blobContainerId,
        IFormFileCollection formFileCollection,
        CancellationToken cancellationToken);

    Task DeleteAttachmentsAsync(Guid blobContainerId);
}
