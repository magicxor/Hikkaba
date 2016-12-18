using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Entities.Attachments.Base;

namespace Hikkaba.Common.Entities.Attachments
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
