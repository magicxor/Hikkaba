using Hikkaba.Infrastructure.Models.Attachments.Base;

namespace Hikkaba.Infrastructure.Models.Attachments.Concrete;

public sealed class PictureModel : FileAttachmentModel
{
    public required int Width { get; set; }
    public required int Height { get; set; }
    public required string ThumbnailExtension { get; set; }
    public required int ThumbnailWidth { get; set; }
    public required int ThumbnailHeight { get; set; }
}
