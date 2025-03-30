using Hikkaba.Shared.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hikkaba.Data.Entities.Attachments.Base;

public abstract class FileAttachment : Attachment
{
    [Required]
    [MaxLength(Defaults.MaxFileNameLength)]
    public required string FileNameWithoutExtension { get; set; }

    [Required]
    [MaxLength(Defaults.MaxFileExtensionLength)]
    public required string FileExtension { get; set; }

    [Required]
    [Range(0, long.MaxValue)]
    public required long FileSize { get; set; }

    [Required]
    [MinLength(Defaults.MaxFileHashBytesLength)]
    [MaxLength(Defaults.MaxFileHashBytesLength)]
    [Column(TypeName = "binary(32)")]
    public required byte[] FileHash { get; set; }
}
