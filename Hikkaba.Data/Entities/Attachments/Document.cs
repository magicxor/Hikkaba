using Hikkaba.Data.Entities.Attachments.Base;
using AttachmentType = Hikkaba.Common.Enums.AttachmentType;

namespace Hikkaba.Data.Entities.Attachments;

public class Document : FileAttachment
{
    public Document()
    {
        AttachmentType = AttachmentType.Document;
    }
}
