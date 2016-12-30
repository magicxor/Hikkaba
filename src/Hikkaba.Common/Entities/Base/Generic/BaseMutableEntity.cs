using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Attributes;

namespace Hikkaba.Common.Entities.Base
{
    public interface IBaseMutableEntity<TPrimaryKey>: IBaseEntity<TPrimaryKey>
    {
        bool IsDeleted { get; set; }
        DateTime Created { get; set; }
        DateTime? Modified { get; set; }
        ApplicationUser CreatedBy { get; set; }
        ApplicationUser ModifiedBy { get; set; }
    }

    public abstract class BaseMutableEntity<TPrimaryKey> : BaseEntity<TPrimaryKey>, IBaseMutableEntity<TPrimaryKey>
    {
        [Required]
        public bool IsDeleted { get; set; }

        [Required]
        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime? Modified { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser ModifiedBy { get; set; }
    }
}