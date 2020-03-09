using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Entities.Attachments.Base;
using Hikkaba.Data.Entities.Base.Current;

namespace Hikkaba.Data.Entities
{
    [Table("Posts")]
    public class Post: BaseMutableEntity
    {
        [Required]
        public bool IsSageEnabled { get; set; }

        [MinLength(Defaults.MinMessageLength)]
        [MaxLength(Defaults.MaxMessageLength)]
        public string Message { get; set; }

        [Required]
        [MaxLength(Defaults.MaxIpAddressLength)]
        public string UserIpAddress { get; set; }

        [Required]
        [MaxLength(Defaults.MaxUserAgentLength)]
        public string UserAgent { get; set; }

        [Required]
        public virtual Thread Thread { get; set; }
        
        public virtual ICollection<Attachment> Attachments { get; set; }
    }
}
