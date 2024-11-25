using System;

namespace Hikkaba.Models.Dto.Base.Generic;

public interface IBaseMutableDto<TPrimaryKey>: IBaseDto<TPrimaryKey>
{
    bool IsDeleted { get; set; }
    DateTime Created { get; set; }
    DateTime? Modified { get; set; }
    ApplicationUserDto CreatedBy { get; set; }
    ApplicationUserDto ModifiedBy { get; set; }
}