using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Entities.Attachments.Base;

namespace Hikkaba.Data.Entities.Attachments
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
