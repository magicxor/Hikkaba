namespace Hikkaba.Data.Entities.Base.Generic;

public interface IBaseEntity<TPrimaryKey>
{
    TPrimaryKey Id { get; set; }
    TPrimaryKey GenerateNewId();
}

public abstract class BaseEntity<TPrimaryKey>: IBaseEntity<TPrimaryKey>
{
    public abstract TPrimaryKey Id { get; set; }
    public abstract TPrimaryKey GenerateNewId();
}