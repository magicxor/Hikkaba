using Hikkaba.Data.Entities.Base.Generic;

namespace Hikkaba.Data.Entities.Base.Current;

public interface IBaseMutableEntity : IBaseEntity, IBaseMutableEntity<TPrimaryKey> { }