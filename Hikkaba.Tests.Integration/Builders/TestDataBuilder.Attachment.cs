using Hikkaba.Data.Entities.Attachments;

namespace Hikkaba.Tests.Integration.Builders;

internal sealed partial class TestDataBuilder
{
    public TestDataBuilder WithAudio(
        string fileNameWithoutExtension = "test_audio",
        string fileExtension = ".mp3",
        long fileSize = 1024,
        string fileContentType = "audio/mpeg",
        string? title = "Test Title",
        string? album = "Test Album",
        string? artist = "Test Artist",
        int? durationSeconds = 180)
    {
        EnsureLastPostExists();
        var audio = new Audio
        {
            BlobId = _guidGenerator.GenerateSeededGuid(),
            Post = _lastPost!,
            FileNameWithoutExtension = fileNameWithoutExtension,
            FileExtension = fileExtension,
            FileSize = fileSize,
            FileContentType = fileContentType,
            FileHash = new byte[32],
            Title = title,
            Album = album,
            Artist = artist,
            DurationSeconds = durationSeconds,
        };
        _dbContext.Audios.Add(audio);
        _lastPost!.Audios.Add(audio);
        return this;
    }

    public TestDataBuilder WithDocument(
        string fileNameWithoutExtension = "test_document",
        string fileExtension = ".pdf",
        long fileSize = 2048,
        string fileContentType = "application/pdf")
    {
        EnsureLastPostExists();
        var document = new Document
        {
            BlobId = _guidGenerator.GenerateSeededGuid(),
            Post = _lastPost!,
            FileNameWithoutExtension = fileNameWithoutExtension,
            FileExtension = fileExtension,
            FileSize = fileSize,
            FileContentType = fileContentType,
            FileHash = new byte[32],
        };
        _dbContext.Documents.Add(document);
        _lastPost!.Documents.Add(document);
        return this;
    }

    public TestDataBuilder WithNotice(string text)
    {
        EnsureLastPostExists();
        EnsureAdminExists();
        var notice = new Notice
        {
            BlobId = _guidGenerator.GenerateSeededGuid(),
            Post = _lastPost!,
            Text = text,
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime,
            CreatedBy = _admin!,
        };
        _dbContext.Notices.Add(notice);
        _lastPost!.Notices.Add(notice);
        return this;
    }

    public TestDataBuilder WithPicture(
        string fileNameWithoutExtension = "test_picture",
        string fileExtension = ".jpg",
        long fileSize = 4096,
        string fileContentType = "image/jpeg",
        int width = 800,
        int height = 600,
        string thumbnailExtension = ".jpg",
        int thumbnailWidth = 200,
        int thumbnailHeight = 150)
    {
        EnsureLastPostExists();
        var picture = new Picture
        {
            BlobId = _guidGenerator.GenerateSeededGuid(),
            Post = _lastPost!,
            FileNameWithoutExtension = fileNameWithoutExtension,
            FileExtension = fileExtension,
            FileSize = fileSize,
            FileContentType = fileContentType,
            FileHash = new byte[32],
            Width = width,
            Height = height,
            ThumbnailExtension = thumbnailExtension,
            ThumbnailWidth = thumbnailWidth,
            ThumbnailHeight = thumbnailHeight,
        };
        _dbContext.Pictures.Add(picture);
        _lastPost!.Pictures.Add(picture);
        return this;
    }

    public TestDataBuilder WithVideo(
        string fileNameWithoutExtension = "test_video",
        string fileExtension = ".mp4",
        long fileSize = 8192,
        string fileContentType = "video/mp4")
    {
        EnsureLastPostExists();
        var video = new Video
        {
            BlobId = _guidGenerator.GenerateSeededGuid(),
            Post = _lastPost!,
            FileNameWithoutExtension = fileNameWithoutExtension,
            FileExtension = fileExtension,
            FileSize = fileSize,
            FileContentType = fileContentType,
            FileHash = new byte[32],
        };
        _dbContext.Videos.Add(video);
        _lastPost!.Videos.Add(video);
        return this;
    }
}
