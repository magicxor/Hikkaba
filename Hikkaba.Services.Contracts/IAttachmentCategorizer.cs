using Hikkaba.Common.Enums;

namespace Hikkaba.Services.Contracts;

public interface IAttachmentCategorizer
{
    bool IsPictureExtensionSupported(string extension);
    AttachmentType GetAttachmentType(string extension);
}
