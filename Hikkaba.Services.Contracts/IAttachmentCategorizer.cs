using System;

namespace Hikkaba.Services.Contracts;

public interface IAttachmentCategorizer
{
    bool IsPictureExtensionSupported(string extension);
    Type GetAttachmentType(string fileName);
}
