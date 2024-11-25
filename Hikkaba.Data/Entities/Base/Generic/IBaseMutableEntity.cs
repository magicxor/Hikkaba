using System;

namespace Hikkaba.Data.Entities.Base.Generic;

public interface IBaseMutableEntity<TPrimaryKey> : IBaseEntity<TPrimaryKey>
{
    bool IsDeleted { get; set; }
    DateTime Created { get; set; }
    DateTime? Modified { get; set; }
    ApplicationUser CreatedBy { get; set; }
    ApplicationUser ModifiedBy { get; set; }
}