using System;

namespace Hikkaba.Common.Dto.Base
{
    public interface IBaseGuidMutableDto: IBaseMutableDto<Guid> { }
    public abstract class BaseGuidMutableDto : BaseMutableDto<Guid>, IBaseGuidMutableDto { }
}