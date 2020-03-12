using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Common.Attributes;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Entities.Base.Current;

namespace Hikkaba.Data.Entities
{
    [Table("Bans")]
    public class Ban: BaseMutableEntity
    {
        [Required]
        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime Start { get; set; }

        [Required]
        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime End { get; set; }
        
        [Required]
        [MaxLength(Defaults.MaxIpAddressLength)]
        public string LowerIpAddress { get; set; }

        [Required]
        [MaxLength(Defaults.MaxIpAddressLength)]
        public string UpperIpAddress { get; set; }

        [Required]
        [MaxLength(Defaults.MaxReasonLength)]
        public string Reason { get; set; }

        public virtual Post RelatedPost { get; set; }

        public virtual Category Category { get; set; }
    }
}
