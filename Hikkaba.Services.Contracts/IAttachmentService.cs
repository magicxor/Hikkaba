using Hikkaba.Infrastructure.Models.Attachments;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Services.Contracts;

public interface IAttachmentService
{
    Task<FileAttachmentCollection> UploadAttachmentsAsync(
        Guid blobContainerId,
        IFormFileCollection formFileCollection,
        CancellationToken cancellationToken);

    Task DeleteAttachmentsAsync(Guid blobContainerId);
}
