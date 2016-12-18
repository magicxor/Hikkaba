using Hikkaba.Common.Dto.Attachments.Base;

namespace Hikkaba.Common.Dto.Attachments
{
    public class PictureDto : FileAttachmentDto
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}