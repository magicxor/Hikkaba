using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Entities.Base;

namespace Hikkaba.Common.Entities
{
    [Table("Bans")]
    public class Ban: BaseMutableEntity
    {
        [Required]
        public DateTime Start { get; set; }

        [Required]
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
