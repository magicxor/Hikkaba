using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Data.Entities.Attachments.Base
{
    public abstract class FileAttachment: Attachment
    {
        [Required]
        public string FileName { get; set; }

        [Required]
        public string FileExtension { get; set; }

        [Required]
        public long Size { get; set; }

        [Required]
        public string Hash { get; set; }
    }
}
