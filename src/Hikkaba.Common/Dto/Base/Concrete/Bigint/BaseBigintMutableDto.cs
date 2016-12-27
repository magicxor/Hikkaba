using System;

namespace Hikkaba.Common.Dto.Base
{
    public interface IBaseBigintMutableDto: IBaseMutableDto<long> { }
    public abstract class BaseBigintMutableDto : BaseMutableDto<long>, IBaseBigintMutableDto { }
}