namespace Hikkaba.Infrastructure.Models.Attachments.Base;

public abstract class FileAttachmentModel : AttachmentModel
{
    public required string FileName { get; set; }
    public required string FileExtension { get; set; }
    public required long FileSize { get; set; }
    public required byte[] FileHash { get; set; }
    public required Guid BlobId { get; set; }
    public required Guid BlobContainerId { get; set; }
}
