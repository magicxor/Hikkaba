namespace Hikkaba.Data.Entities.Base.Generic;

public abstract class BaseEntity<TPrimaryKey>: IBaseEntity<TPrimaryKey>
{
    public abstract TPrimaryKey Id { get; set; }
    public abstract TPrimaryKey GenerateNewId();
}