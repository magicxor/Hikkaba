namespace Hikkaba.Models.Dto.Attachments.Base;

public abstract class FileAttachmentDto : AttachmentDto
{
    public string FileName { get; set; }
    public string FileExtension { get; set; }
    public long Size { get; set; }
    public string Hash { get; set; }
}