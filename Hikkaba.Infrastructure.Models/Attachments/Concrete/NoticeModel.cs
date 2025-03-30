using Hikkaba.Infrastructure.Models.Attachments.Base;

namespace Hikkaba.Infrastructure.Models.Attachments.Concrete;

public class NoticeModel : AttachmentModel
{
    public required string Text { get; set; }
}
