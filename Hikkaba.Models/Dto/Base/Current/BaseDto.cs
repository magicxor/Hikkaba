using Hikkaba.Models.Dto.Base.Concrete.Guid;

namespace Hikkaba.Models.Dto.Base.Current
{
    public interface IBaseDto: IBaseGuidDto { }
    public abstract class BaseDto : BaseGuidDto, IBaseDto { }
}
