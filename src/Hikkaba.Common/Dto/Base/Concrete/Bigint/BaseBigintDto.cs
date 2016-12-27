using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Common.Dto.Base
{
    public interface IBaseBigintDto : IBaseDto<long> { }
    public abstract class BaseBigintDto : BaseDto<long>, IBaseBigintDto { }
}
