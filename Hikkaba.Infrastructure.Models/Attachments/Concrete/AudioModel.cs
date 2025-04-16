using Hikkaba.Infrastructure.Models.Attachments.Base;

namespace Hikkaba.Infrastructure.Models.Attachments.Concrete;

public sealed class AudioModel : FileAttachmentModel
{
    public required string? Title { get; set; }

    public required string? Album { get; set; }

    public required string? Artist { get; set; }

    public required int? DurationSeconds { get; set; }
}
