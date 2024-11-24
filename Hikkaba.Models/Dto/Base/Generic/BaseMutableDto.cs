using System;
using Hikkaba.Common.Attributes;

namespace Hikkaba.Models.Dto.Base.Generic;

public interface IBaseMutableDto<TPrimaryKey>: IBaseDto<TPrimaryKey>
{
    bool IsDeleted { get; set; }
    DateTime Created { get; set; }
    DateTime? Modified { get; set; }
    ApplicationUserDto CreatedBy { get; set; }
    ApplicationUserDto ModifiedBy { get; set; }
}

public abstract class BaseMutableDto<TPrimaryKey>: BaseDto<TPrimaryKey>, IBaseMutableDto<TPrimaryKey>
{
    public bool IsDeleted { get; set; }

    [DateTimeKind(DateTimeKind.Utc)]
    public DateTime Created { get; set; }

    [DateTimeKind(DateTimeKind.Utc)]
    public DateTime? Modified { get; set; }

    public ApplicationUserDto CreatedBy { get; set; }

    public ApplicationUserDto ModifiedBy { get; set; }
}