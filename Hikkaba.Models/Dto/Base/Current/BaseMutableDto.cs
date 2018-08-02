using Hikkaba.Models.Dto.Base.Concrete.Guid;

namespace Hikkaba.Models.Dto.Base.Current
{
    public interface IBaseMutableDto: IBaseGuidMutableDto { }
    public abstract class BaseMutableDto : BaseGuidMutableDto, IBaseMutableDto { }
}