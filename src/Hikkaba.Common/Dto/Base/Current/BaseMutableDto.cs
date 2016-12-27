using System;

namespace Hikkaba.Common.Dto.Base
{
    public interface IBaseMutableDto: IBaseGuidMutableDto { }
    public abstract class BaseMutableDto : BaseGuidMutableDto, IBaseMutableDto { }
}