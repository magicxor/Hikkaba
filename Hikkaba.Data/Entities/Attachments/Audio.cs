using System.ComponentModel.DataAnnotations;
using Hikkaba.Data.Entities.Attachments.Base;
using Hikkaba.Shared.Constants;
using AttachmentType = Hikkaba.Shared.Enums.AttachmentType;

namespace Hikkaba.Data.Entities.Attachments;

public class Audio : FileAttachment
{
    [MaxLength(Defaults.MaxAudioMetadataLength)]
    public required string? Title { get; set; }

    [MaxLength(Defaults.MaxAudioMetadataLength)]
    public required string? Album { get; set; }

    [MaxLength(Defaults.MaxAudioMetadataLength)]
    public required string? Artist { get; set; }

    [Range(0, int.MaxValue)]
    public required int? DurationSeconds { get; set; }

    public Audio()
    {
        AttachmentType = AttachmentType.Audio;
    }
}
