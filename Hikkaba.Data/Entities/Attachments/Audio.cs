using Hikkaba.Data.Entities.Attachments.Base;
using AttachmentType = Hikkaba.Shared.Enums.AttachmentType;

namespace Hikkaba.Data.Entities.Attachments;

public class Audio : FileAttachment
{
    public Audio()
    {
        AttachmentType = AttachmentType.Audio;
    }
}
