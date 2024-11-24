using TPrimaryKey = System.Guid;
using Hikkaba.Models.Dto.Base.Generic;

namespace Hikkaba.Models.Dto.Base.Current;

public interface IBaseDto : IBaseDto<TPrimaryKey> { }
public abstract class BaseDto : BaseDto<TPrimaryKey>, IBaseDto { }

public interface IBaseMutableDto : IBaseMutableDto<TPrimaryKey> { }
public abstract class BaseMutableDto : BaseMutableDto<TPrimaryKey>, IBaseMutableDto { }