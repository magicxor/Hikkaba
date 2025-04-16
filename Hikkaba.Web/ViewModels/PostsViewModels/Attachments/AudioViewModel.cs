using System;

namespace Hikkaba.Web.ViewModels.PostsViewModels.Attachments;

public sealed class AudioViewModel
{
    public required long Id { get; set; }
    public required long PostId { get; set; }
    public required long ThreadId { get; set; }
    public required Guid BlobContainerId { get; set; }
    public required Guid BlobId { get; set; }
    public required string FileName { get; set; }
    public required string FileExtension { get; set; }
    public required long FileSize { get; set; }
    public required string FileContentType { get; set; }
    public required string FileHash { get; set; }

    public required string? Title { get; set; }

    public required string? Album { get; set; }

    public required string? Artist { get; set; }

    public required int? DurationSeconds { get; set; }
}
