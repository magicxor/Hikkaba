using Hikkaba.Data.Entities.Base.Concrete.Guid;

namespace Hikkaba.Data.Entities.Base.Current
{
    public interface IBaseEntity: IBaseGuidEntity { }
    public abstract class BaseEntity: BaseGuidEntity, IBaseEntity { }
}
