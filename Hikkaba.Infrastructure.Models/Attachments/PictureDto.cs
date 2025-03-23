using Hikkaba.Infrastructure.Models.Attachments.Base;

namespace Hikkaba.Infrastructure.Models.Attachments;

public class PictureDto : FileAttachmentDto
{
    public required int Width { get; set; }
    public required int Height { get; set; }
}
