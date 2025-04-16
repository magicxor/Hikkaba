using Hikkaba.Data.Aggregates;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Enums;
using Hikkaba.Shared.Extensions;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public sealed class AttachmentRepository : IAttachmentRepository
{
    public AttachmentCollections ToAttachmentEntities(FileAttachmentContainerCollection inputFiles)
    {
        return new AttachmentCollections
        {
            Audios = inputFiles.OfType<AudioFileAttachmentStreamContainer>()
                .Where(f => f.AttachmentType == AttachmentType.Audio)
                .Select(f => new Audio
                {
                    BlobId = f.BlobId,
                    AttachmentType = f.AttachmentType,
                    FileNameWithoutExtension = f.FileNameWithoutExtension.TryLeft(Defaults.MaxFileNameLength),
                    FileExtension = f.FileExtension.TryLeft(Defaults.MaxFileExtensionLength),
                    FileSize = f.FileSize,
                    FileContentType = f.FileContentType.TryLeft(Defaults.MaxFileContentTypeLength),
                    FileHash = f.FileHash,
                    Title = f.Title,
                    Album = f.Album,
                    Artist = f.Artist,
                    DurationSeconds = f.DurationSeconds,
                })
                .ToList(),
            Documents = inputFiles.Where(f => f.AttachmentType == AttachmentType.Document)
                .Select(f => new Document
                {
                    BlobId = f.BlobId,
                    AttachmentType = f.AttachmentType,
                    FileNameWithoutExtension = f.FileNameWithoutExtension.TryLeft(Defaults.MaxFileNameLength),
                    FileExtension = f.FileExtension.TryLeft(Defaults.MaxFileExtensionLength),
                    FileSize = f.FileSize,
                    FileContentType = f.FileContentType.TryLeft(Defaults.MaxFileContentTypeLength),
                    FileHash = f.FileHash,
                })
                .ToList(),
            Pictures = inputFiles.OfType<PictureFileAttachmentStreamContainer>()
                .Where(f => f.AttachmentType == AttachmentType.Picture)
                .Select(f => new Picture
                {
                    BlobId = f.BlobId,
                    AttachmentType = f.AttachmentType,
                    FileNameWithoutExtension = f.FileNameWithoutExtension.TryLeft(Defaults.MaxFileNameLength),
                    FileExtension = f.FileExtension.TryLeft(Defaults.MaxFileExtensionLength),
                    FileSize = f.FileSize,
                    FileContentType = f.FileContentType.TryLeft(Defaults.MaxFileContentTypeLength),
                    FileHash = f.FileHash,
                    Width = f.Width,
                    Height = f.Height,
                    ThumbnailExtension = f.ThumbnailStreamContainer?.Extension ?? string.Empty,
                    ThumbnailWidth = f.ThumbnailStreamContainer?.Width ?? 0,
                    ThumbnailHeight = f.ThumbnailStreamContainer?.Height ?? 0,
                })
                .ToList(),
            Videos = inputFiles.Where(f => f.AttachmentType == AttachmentType.Video)
                .Select(f => new Video
                {
                    BlobId = f.BlobId,
                    AttachmentType = f.AttachmentType,
                    FileNameWithoutExtension = f.FileNameWithoutExtension.TryLeft(Defaults.MaxFileNameLength),
                    FileExtension = f.FileExtension.TryLeft(Defaults.MaxFileExtensionLength),
                    FileSize = f.FileSize,
                    FileContentType = f.FileContentType.TryLeft(Defaults.MaxFileContentTypeLength),
                    FileHash = f.FileHash,
                })
                .ToList(),
        };
    }
}
