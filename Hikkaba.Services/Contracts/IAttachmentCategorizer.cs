using Hikkaba.Models.Dto.Attachments.Base;

namespace Hikkaba.Services.Contracts;

public interface IAttachmentCategorizer
{
    bool IsPictureExtensionSupported(string extension);
    FileAttachmentDto CreateAttachmentDto(string fileName);
}