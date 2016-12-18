using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Common.Entities.Attachments.Base;

namespace Hikkaba.Common.Entities.Attachments
{
    [Table("Pictures")]
    public class Picture: FileAttachment
    {
        [Required]
        public int Width { get; set; }

        [Required]
        public int Height { get; set; }
    }
}
