using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Common.Entities.Base
{
    public interface IBaseEntity: IBaseGuidEntity { }
    public abstract class BaseEntity: BaseGuidEntity, IBaseEntity { }
}
