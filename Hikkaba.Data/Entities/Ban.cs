using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Common.Attributes;
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
        public string LowerIpAddress { get; set; }

        [Required]
        public string UpperIpAddress { get; set; }

        [Required]
        public string Reason { get; set; }

        public virtual Post RelatedPost { get; set; }

        public virtual Category Category { get; set; }
    }
}
