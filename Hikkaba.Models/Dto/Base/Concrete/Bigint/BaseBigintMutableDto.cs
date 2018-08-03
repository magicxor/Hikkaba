using Hikkaba.Models.Dto.Base.Generic;

namespace Hikkaba.Models.Dto.Base.Concrete.Bigint
{
    public interface IBaseBigintMutableDto: IBaseMutableDto<long> { }
    public abstract class BaseBigintMutableDto : BaseMutableDto<long>, IBaseBigintMutableDto { }
}