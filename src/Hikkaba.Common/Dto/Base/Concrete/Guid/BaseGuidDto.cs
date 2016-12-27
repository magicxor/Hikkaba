using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Common.Dto.Base
{
    public interface IBaseGuidDto: IBaseDto<Guid> { }
    public abstract class BaseGuidDto : BaseDto<Guid>, IBaseGuidDto { }
}
