using Hikkaba.Infrastructure.Models.Attachments.Base;

namespace Hikkaba.Infrastructure.Models.Attachments.Concrete;

public class PictureModel : FileAttachmentModel
{
    public required int Width { get; set; }
    public required int Height { get; set; }
}
