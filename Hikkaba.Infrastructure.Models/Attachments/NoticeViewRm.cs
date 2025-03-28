using Hikkaba.Infrastructure.Models.Attachments.Base;

namespace Hikkaba.Infrastructure.Models.Attachments;

public class NoticeViewRm : AttachmentViewRm
{
    public required string Text { get; set; }
}
