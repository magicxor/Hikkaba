using System.ComponentModel.DataAnnotations;
using Hikkaba.Data.Entities.Attachments.Base;

namespace Hikkaba.Data.Entities.Attachments
{
    public class Picture: FileAttachment
    {
        [Required]
        public int Width { get; set; }

        [Required]
        public int Height { get; set; }
    }
}
