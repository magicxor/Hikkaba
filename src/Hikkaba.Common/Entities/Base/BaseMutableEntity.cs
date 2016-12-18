using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Common.Entities.Base
{
    public abstract class BaseMutableEntity : BaseEntity
    {
        [Required]
        public bool IsDeleted { get; set; }

        [Required]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? Modified { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser ModifiedBy { get; set; }
    }
}
