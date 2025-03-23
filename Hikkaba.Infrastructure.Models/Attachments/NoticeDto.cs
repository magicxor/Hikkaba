using Hikkaba.Infrastructure.Models.Attachments.Base;

namespace Hikkaba.Infrastructure.Models.Attachments;

public class NoticeDto : AttachmentDto
{
    public required string Text { get; set; }
}
