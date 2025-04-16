namespace Hikkaba.Infrastructure.Models.Attachments.StreamContainers;

public class AudioFileAttachmentStreamContainer : FileAttachmentStreamContainer
{
    public required string? Album { get; set; }

    public required string? Artist { get; set; }

    public required int? DurationSeconds { get; set; }
}
