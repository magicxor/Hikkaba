using System.ComponentModel.DataAnnotations;
using Hikkaba.Data.Entities.Attachments.Base;
using Hikkaba.Shared.Constants;
using AttachmentType = Hikkaba.Shared.Enums.AttachmentType;

namespace Hikkaba.Data.Entities.Attachments;

public class Picture : FileAttachment
{
    [Required]
    [Range(0, int.MaxValue)]
    public required int Width { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public required int Height { get; set; }

    [Required]
    [MaxLength(Defaults.MaxFileExtensionLength)]
    public required string ThumbnailExtension { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public required int ThumbnailWidth { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public required int ThumbnailHeight { get; set; }

    public Picture()
    {
        AttachmentType = AttachmentType.Picture;
    }
}
