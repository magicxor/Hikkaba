using System.ComponentModel.DataAnnotations;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Entities.Attachments.Base;

namespace Hikkaba.Data.Entities.Attachments
{
    public class Notice: Attachment
    {
        [Required]
        [MaxLength(Defaults.MaxNoticeLength)]
        public string Text { get; set; }

        [Required]
        public virtual ApplicationUser Author { get; set; }
    }
}
