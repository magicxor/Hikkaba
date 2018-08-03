using Hikkaba.Models.Dto.Base.Generic;

namespace Hikkaba.Models.Dto.Base.Concrete.Guid
{
    public interface IBaseGuidMutableDto: IBaseMutableDto<System.Guid> { }
    public abstract class BaseGuidMutableDto : BaseMutableDto<System.Guid>, IBaseGuidMutableDto { }
}