using Hikkaba.Shared.Enums;

namespace Hikkaba.Application.Contracts;

public interface IAttachmentCategorizer
{
    bool IsPictureExtensionSupported(string extension);
    AttachmentType GetAttachmentType(string extension);
}
