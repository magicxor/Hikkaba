using Hikkaba.Data.Aggregates;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Enums;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public class AttachmentRepository : IAttachmentRepository
{
    public AttachmentCollections ToAttachmentEntities(FileAttachmentContainerCollection inputFiles)
    {
        return new AttachmentCollections
        {
            Audios = inputFiles.Where(f => f.AttachmentType == AttachmentType.Audio)
                .Select(f => new Audio
                {
                    BlobId = f.BlobId,
                    AttachmentType = f.AttachmentType,
                    FileNameWithoutExtension = f.FileNameWithoutExtension,
                    FileExtension = f.FileExtension,
                    FileSize = f.FileSize,
                    FileContentType = f.FileContentType,
                    FileHash = f.FileHash,
                })
                .ToList(),
            Documents = inputFiles.Where(f => f.AttachmentType == AttachmentType.Document)
                .Select(f => new Document
                {
                    BlobId = f.BlobId,
                    AttachmentType = f.AttachmentType,
                    FileNameWithoutExtension = f.FileNameWithoutExtension,
                    FileExtension = f.FileExtension,
                    FileSize = f.FileSize,
                    FileContentType = f.FileContentType,
                    FileHash = f.FileHash,
                })
                .ToList(),
            Pictures = inputFiles.OfType<PictureFileAttachmentStreamContainer>()
                .Where(f => f.AttachmentType == AttachmentType.Picture)
                .Select(f => new Picture
                {
                    BlobId = f.BlobId,
                    AttachmentType = f.AttachmentType,
                    FileNameWithoutExtension = f.FileNameWithoutExtension,
                    FileExtension = f.FileExtension,
                    FileSize = f.FileSize,
                    FileContentType = f.FileContentType,
                    FileHash = f.FileHash,
                    Width = f.Width,
                    Height = f.Height,
                })
                .ToList(),
            Videos = inputFiles.Where(f => f.AttachmentType == AttachmentType.Video)
                .Select(f => new Video
                {
                    BlobId = f.BlobId,
                    AttachmentType = f.AttachmentType,
                    FileNameWithoutExtension = f.FileNameWithoutExtension,
                    FileExtension = f.FileExtension,
                    FileSize = f.FileSize,
                    FileContentType = f.FileContentType,
                    FileHash = f.FileHash,
                })
                .ToList(),
        };
    }
}
