using System.ComponentModel.DataAnnotations;
using Hikkaba.Data.Entities.Attachments.Base;
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

    public Picture()
    {
        AttachmentType = AttachmentType.Picture;
    }
}
