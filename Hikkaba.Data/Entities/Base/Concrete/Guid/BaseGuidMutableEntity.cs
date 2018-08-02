using Hikkaba.Data.Entities.Base.Generic;

namespace Hikkaba.Data.Entities.Base.Concrete.Guid
{
    public interface IBaseGuidMutableEntity: IBaseMutableEntity<System.Guid> { }
    public abstract class BaseGuidMutableEntity : BaseMutableEntity<System.Guid>, IBaseGuidMutableEntity
    {
    }
}