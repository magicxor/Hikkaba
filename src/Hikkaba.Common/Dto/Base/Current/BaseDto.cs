using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Common.Dto.Base
{
    public interface IBaseDto: IBaseGuidDto { }
    public abstract class BaseDto : BaseGuidDto, IBaseDto { }
}
