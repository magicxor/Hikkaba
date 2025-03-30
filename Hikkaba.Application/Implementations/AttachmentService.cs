using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Enums;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using TwentyTwenty.Storage;

namespace Hikkaba.Application.Implementations;

public class AttachmentService : IAttachmentService
{
    private readonly IOptions<HikkabaConfiguration> _settings;
    private readonly IAttachmentCategorizer _attachmentCategorizer;
    private readonly IStorageProvider _storageProvider;
    private readonly IHashService _hashService;
    private readonly IThumbnailGenerator _thumbnailGenerator;

    public AttachmentService(
        IOptions<HikkabaConfiguration> settings,
        IAttachmentCategorizer attachmentCategorizer,
        IStorageProvider storageProvider,
        IHashService hashService,
        IThumbnailGenerator thumbnailGenerator)
    {
        _settings = settings;
        _attachmentCategorizer = attachmentCategorizer;
        _storageProvider = storageProvider;
        _hashService = hashService;
        _thumbnailGenerator = thumbnailGenerator;
    }

    private async Task<FileAttachmentStreamContainer> ConvertToFileAttachmentSmAsync(
        IFormFile formFile,
        CancellationToken cancellationToken)
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var tempFileStream = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose);
        await formFile.OpenReadStream().CopyToAsync(tempFileStream, cancellationToken);
        tempFileStream.Position = 0;

        var blobId = Guid.NewGuid();
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(formFile.FileName);
        var fileExtension = Path.GetExtension(formFile.FileName).ToLowerInvariant().TrimStart('.');
        var fileNameWithExtension = fileNameWithoutExtension + "." + fileExtension;
        var fileSize = formFile.Length;
        var attachmentType = _attachmentCategorizer.GetAttachmentType(fileExtension);
        var fileHash = _hashService.GetHashBytes(formFile.OpenReadStream());
        var contentType = formFile.ContentType;

        if (attachmentType != AttachmentType.Picture)
        {
            return new FileAttachmentStreamContainer
            {
                BlobId = blobId,
                AttachmentType = attachmentType,
                FileNameWithoutExtension = fileNameWithoutExtension,
                FileExtension = fileExtension,
                FileNameWithExtension = fileNameWithExtension,
                FileSize = fileSize,
                ContentType = contentType,
                FileStream = tempFileStream,
                FileHash = fileHash,
            };
        }

        if (!_attachmentCategorizer.IsPictureExtensionSupported(fileExtension))
        {
            return new PictureFileAttachmentStreamContainer
            {
                BlobId = blobId,
                AttachmentType = attachmentType,
                FileNameWithoutExtension = fileNameWithoutExtension,
                FileExtension = fileExtension,
                FileNameWithExtension = fileNameWithExtension,
                FileSize = fileSize,
                ContentType = contentType,
                FileStream = tempFileStream,
                FileHash = fileHash,
                Width = 0,
                Height = 0,
                ThumbnailStreamContainer = null,
            };
        }

        using var image = await Image.LoadAsync(formFile.OpenReadStream(), cancellationToken);
        var width = image.Width;
        var height = image.Height;

        var thumbnail = await _thumbnailGenerator.GenerateThumbnailAsync(
            image,
            _settings.Value.ThumbnailsMaxWidth,
            _settings.Value.ThumbnailsMaxHeight,
            cancellationToken);

        thumbnail.ContentStream.Position = 0;
        tempFileStream.Position = 0;

        return new PictureFileAttachmentStreamContainer
        {
            BlobId = blobId,
            AttachmentType = attachmentType,
            FileNameWithoutExtension = fileNameWithoutExtension,
            FileExtension = fileExtension,
            FileNameWithExtension = fileNameWithExtension,
            FileSize = fileSize,
            ContentType = contentType,
            FileStream = tempFileStream,
            FileHash = fileHash,
            Width = width,
            Height = height,
            ThumbnailStreamContainer = thumbnail,
        };
    }

    [MustDisposeResource]
    public async Task<FileAttachmentContainerCollection> UploadAttachmentsAsync(
        Guid blobContainerId,
        IFormFileCollection formFileCollection,
        CancellationToken cancellationToken)
    {
        var attachments = new FileAttachmentContainerCollection();

        foreach (var formFile in formFileCollection)
        {
            attachments.Add(await ConvertToFileAttachmentSmAsync(formFile, cancellationToken));
        }

        var fileUploadTasks = attachments.Select(attachment =>
            _storageProvider.SaveBlobStreamAsync(
                blobContainerId.ToString(),
                attachment.BlobId.ToString(),
                attachment.FileStream,
                closeStream: false,
                length: attachment.FileSize));

        var thumbnailUploadTasks = attachments
            .OfType<PictureFileAttachmentStreamContainer>()
            .Where(attachment => attachment.ThumbnailStreamContainer != null)
            .Select(attachment =>
                _storageProvider.SaveBlobStreamAsync(
                    blobContainerId.ToString(),
                    attachment.BlobId + Defaults.ThumbnailPostfix,
                    attachment.ThumbnailStreamContainer!.ContentStream,
                    closeStream: false));

        await Task.WhenAll(fileUploadTasks.Concat(thumbnailUploadTasks));

        return attachments;
    }

    public async Task DeleteAttachmentsAsync(Guid blobContainerId)
    {
        await _storageProvider.DeleteContainerAsync(blobContainerId.ToString());
    }
}
