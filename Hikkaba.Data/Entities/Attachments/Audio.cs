using System.ComponentModel.DataAnnotations;
using Hikkaba.Data.Entities.Attachments.Base;
using Hikkaba.Shared.Constants;
using AttachmentType = Hikkaba.Shared.Enums.AttachmentType;

namespace Hikkaba.Data.Entities.Attachments;

public class Audio : FileAttachment
{
    [MaxLength(Defaults.MaxAudioMetadataLength)]
    public string? Title { get; set; }

    [MaxLength(Defaults.MaxAudioMetadataLength)]
    public string? Album { get; set; }

    [MaxLength(Defaults.MaxAudioMetadataLength)]
    public string? Artist { get; set; }

    [Range(0, int.MaxValue)]
    public int? DurationMs { get; set; }

    public Audio()
    {
        AttachmentType = AttachmentType.Audio;
    }
}
