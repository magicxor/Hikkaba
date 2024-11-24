using Hikkaba.Models.Dto.Attachments.Base;

namespace Hikkaba.Models.Dto.Attachments;

public class PictureDto : FileAttachmentDto
{
    public int Width { get; set; }
    public int Height { get; set; }
}