using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Common.Entities.Base
{
    public interface IBaseGuidMutableEntity: IBaseMutableEntity<Guid> { }
    public abstract class BaseGuidMutableEntity : BaseMutableEntity<Guid>, IBaseGuidMutableEntity
    {
    }
}