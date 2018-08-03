using Hikkaba.Models.Dto.Base.Generic;

namespace Hikkaba.Models.Dto.Base.Concrete.Bigint
{
    public interface IBaseBigintDto : IBaseDto<long> { }
    public abstract class BaseBigintDto : BaseDto<long>, IBaseBigintDto { }
}
