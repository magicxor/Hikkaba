using System;

namespace Hikkaba.Common.Dto.Base
{
    public abstract class BaseMutableDto: BaseDto
    {
        public bool IsDeleted { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
        public ApplicationUserDto CreatedBy { get; set; }
        public ApplicationUserDto ModifiedBy { get; set; }
    }
}