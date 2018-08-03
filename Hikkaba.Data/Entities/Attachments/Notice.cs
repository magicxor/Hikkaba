using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Entities.Attachments.Base;

namespace Hikkaba.Data.Entities.Attachments
{
    [Table("Notices")]
    public class Notice: Attachment
    {
        [Required]
        public string Text { get; set; }

        [Required]
        public virtual ApplicationUser Author { get; set; }
    }
}
