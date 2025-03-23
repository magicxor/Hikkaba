namespace Hikkaba.Infrastructure.Models.Attachments.Base;

public abstract class AttachmentDto
{
    public required long Id { get; set; }

    public required long PostId { get; set; }

    public required long ThreadId { get; set; }
}
