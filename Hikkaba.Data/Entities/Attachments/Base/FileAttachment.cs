using Hikkaba.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Data.Entities.Attachments.Base;

public abstract class FileAttachment: Attachment
{
    [Required]
    [MaxLength(Defaults.MaxFileNameLength)]
    public string FileName { get; set; }

    [Required]
    [MaxLength(Defaults.MaxFileExtensionLength)]
    public string FileExtension { get; set; }

    [Required]
    public long Size { get; set; }

    [Required]
    [MaxLength(Defaults.MaxFileHashLength)]
    public string Hash { get; set; }
}