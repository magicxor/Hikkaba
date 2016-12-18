using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Common.Entities.Attachments.Base
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
