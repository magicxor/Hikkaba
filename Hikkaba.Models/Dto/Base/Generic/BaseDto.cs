namespace Hikkaba.Models.Dto.Base.Generic;

public abstract class BaseDto<TPrimaryKey> : IBaseDto<TPrimaryKey>
{
    public TPrimaryKey Id { get; set; }
}