using Hikkaba.Infrastructure.Models.Attachments.Base;

namespace Hikkaba.Infrastructure.Models.Attachments.Concrete;

public sealed class NoticeModel : AttachmentModel
{
    public required string Text { get; set; }
}
