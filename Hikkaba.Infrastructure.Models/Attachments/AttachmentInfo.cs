using Hikkaba.Shared.Enums;

namespace Hikkaba.Infrastructure.Models.Attachments;

public sealed class AttachmentInfo
{
    public required Guid BlobId { get; init; }

    public required AttachmentType AttachmentType { get; init; }

    public required string FileNameWithoutExtension { get; init; }

    public required string FileExtension { get; init; }

    public required string FileNameWithExtension { get; init; }

    public required long FileSize { get; init; }

    public required string FileContentType { get; init; }

    public required byte[] FileHash { get; init; }
}
