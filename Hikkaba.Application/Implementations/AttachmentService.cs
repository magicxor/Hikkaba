using System.Diagnostics;
using ATL;
using Hikkaba.Application.Contracts;
using Hikkaba.Application.Telemetry;
using Hikkaba.Infrastructure.Models.Attachments;
using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Enums;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using TwentyTwenty.Storage;

namespace Hikkaba.Application.Implementations;

public sealed class AttachmentService : IAttachmentService
{
    private static readonly DecoderOptions DecoderOptions = new() { MaxFrames = 1 };

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

    private FileAttachmentStreamContainer ConvertToGenericFileAttachment(
        AttachmentInfo attachmentInfo,
        FileStream fileStream)
    {
        return new FileAttachmentStreamContainer
        {
            BlobId = attachmentInfo.BlobId,
            AttachmentType = attachmentInfo.AttachmentType,
            FileNameWithoutExtension = attachmentInfo.FileNameWithoutExtension,
            FileExtension = attachmentInfo.FileExtension,
            FileNameWithExtension = attachmentInfo.FileNameWithExtension,
            FileSize = attachmentInfo.FileSize,
            FileContentType = attachmentInfo.FileContentType,
            FileStream = fileStream,
            FileHash = attachmentInfo.FileHash,
        };
    }

    private PictureFileAttachmentStreamContainer ConvertToUnsupportedPictureAttachment(
        AttachmentInfo attachmentInfo,
        FileStream fileStream)
    {
        return new PictureFileAttachmentStreamContainer
        {
            BlobId = attachmentInfo.BlobId,
            AttachmentType = attachmentInfo.AttachmentType,
            FileNameWithoutExtension = attachmentInfo.FileNameWithoutExtension,
            FileExtension = attachmentInfo.FileExtension,
            FileNameWithExtension = attachmentInfo.FileNameWithExtension,
            FileSize = attachmentInfo.FileSize,
            FileContentType = attachmentInfo.FileContentType,
            FileStream = fileStream,
            FileHash = attachmentInfo.FileHash,
            Width = 0,
            Height = 0,
            ThumbnailStreamContainer = null,
        };
    }

    private async Task<PictureFileAttachmentStreamContainer> ConvertToPictureAttachmentAsync(
        AttachmentInfo attachmentInfo,
        IFormFile formFile,
        FileStream tempFileStream,
        CancellationToken cancellationToken)
    {
        var imageMetadata = await Image.IdentifyAsync(DecoderOptions, formFile.OpenReadStream(), cancellationToken);
        if (imageMetadata.Width > Defaults.MaxImageWidth || imageMetadata.Height > Defaults.MaxImageHeight)
        {
            throw new InvalidOperationException($"Image is too large: {imageMetadata.Width}x{imageMetadata.Height}. Max: {Defaults.MaxImageWidth}x{Defaults.MaxImageHeight}");
        }

        using var image = await Image.LoadAsync(DecoderOptions, formFile.OpenReadStream(), cancellationToken);
        var width = image.Width;
        var height = image.Height;

        var thumbnail = await _thumbnailGenerator.GenerateThumbnailAsync(
            image,
            _settings.Value.ThumbnailsMaxWidth,
            _settings.Value.ThumbnailsMaxHeight,
            cancellationToken);

        thumbnail.ContentStream.Position = 0;

        return new PictureFileAttachmentStreamContainer
        {
            BlobId = attachmentInfo.BlobId,
            AttachmentType = attachmentInfo.AttachmentType,
            FileNameWithoutExtension = attachmentInfo.FileNameWithoutExtension,
            FileExtension = attachmentInfo.FileExtension,
            FileNameWithExtension = attachmentInfo.FileNameWithExtension,
            FileSize = attachmentInfo.FileSize,
            FileContentType = attachmentInfo.FileContentType,
            FileStream = tempFileStream,
            FileHash = attachmentInfo.FileHash,
            Width = width,
            Height = height,
            ThumbnailStreamContainer = thumbnail,
        };
    }

    private AudioFileAttachmentStreamContainer ConvertToAudioAttachment(
        AttachmentInfo attachmentInfo,
        FileStream fileStream)
    {
        var audioTrack = new Track(fileStream);

        return new AudioFileAttachmentStreamContainer
        {
            BlobId = attachmentInfo.BlobId,
            AttachmentType = attachmentInfo.AttachmentType,
            FileNameWithoutExtension = attachmentInfo.FileNameWithoutExtension,
            FileExtension = attachmentInfo.FileExtension,
            FileNameWithExtension = attachmentInfo.FileNameWithExtension,
            FileSize = attachmentInfo.FileSize,
            FileContentType = attachmentInfo.FileContentType,
            FileStream = fileStream,
            Title = audioTrack.Title,
            Album = audioTrack.Album ?? audioTrack.OriginalAlbum,
            Artist = audioTrack.Artist ?? audioTrack.AlbumArtist ?? audioTrack.OriginalArtist,
            DurationSeconds = audioTrack.Duration,
            FileHash = attachmentInfo.FileHash,
        };
    }

    [MustDisposeResource]
    private async Task<FileAttachmentStreamContainer> ConvertToFileAttachmentAsync(
        IFormFile formFile,
        CancellationToken cancellationToken)
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var tempFileStream = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose);

        using (var activity = ApplicationTelemetry.AttachmentServiceSource.StartActivity(
                   "CopyFormFileOnDisk",
                   ActivityKind.Internal,
                   null,
                   tags: [new("fileSize", formFile.Length), new("fileExt", Path.GetExtension(formFile.FileName))]))
        {
            await formFile.OpenReadStream().CopyToAsync(tempFileStream, cancellationToken);
        }

        tempFileStream.Position = 0;

        var fileExtension = Path.GetExtension(formFile.FileName).ToLowerInvariant().TrimStart('.');
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(formFile.FileName);
        var attachmentInfo = new AttachmentInfo
        {
            BlobId = Guid.NewGuid(),
            AttachmentType = _attachmentCategorizer.GetAttachmentType(fileExtension),
            FileNameWithoutExtension = fileNameWithoutExtension,
            FileExtension = fileExtension,
            FileNameWithExtension = fileNameWithoutExtension + "." + fileExtension,
            FileSize = formFile.Length,
            FileContentType = formFile.ContentType,
            FileHash = _hashService.GetHashBytes(formFile.OpenReadStream()),
        };

        return attachmentInfo switch
        {
            { AttachmentType: AttachmentType.Picture } when _attachmentCategorizer.IsPictureExtensionSupported(fileExtension) => await ConvertToPictureAttachmentAsync(
                attachmentInfo,
                formFile,
                tempFileStream,
                cancellationToken),
            { AttachmentType: AttachmentType.Picture } when !_attachmentCategorizer.IsPictureExtensionSupported(fileExtension) => ConvertToUnsupportedPictureAttachment(
                attachmentInfo,
                tempFileStream),
            { AttachmentType: AttachmentType.Audio } => ConvertToAudioAttachment(attachmentInfo, tempFileStream),
            _ => ConvertToGenericFileAttachment(
                attachmentInfo,
                tempFileStream),
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
            attachments.Add(await ConvertToFileAttachmentAsync(formFile, cancellationToken));
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

    public async Task DeleteAttachmentAsync(Guid blobContainerId, Guid blobId)
    {
        await _storageProvider.DeleteBlobAsync(
            blobContainerId.ToString(),
            blobId.ToString());
    }

    public async Task DeleteAttachmentsContainerAsync(Guid blobContainerId)
    {
        await _storageProvider.DeleteContainerAsync(blobContainerId.ToString());
    }
}
