using System;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Common.Attributes;

namespace Hikkaba.Data.Entities.Base.Generic;

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