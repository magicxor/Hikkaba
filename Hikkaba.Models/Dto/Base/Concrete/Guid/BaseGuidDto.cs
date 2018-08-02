using Hikkaba.Models.Dto.Base.Generic;

namespace Hikkaba.Models.Dto.Base.Concrete.Guid
{
    public interface IBaseGuidDto: IBaseDto<System.Guid> { }
    public abstract class BaseGuidDto : BaseDto<System.Guid>, IBaseGuidDto { }
}
