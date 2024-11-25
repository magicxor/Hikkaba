namespace Hikkaba.Data.Entities.Base.Generic;

public interface IBaseEntity<TPrimaryKey>
{
    TPrimaryKey Id { get; set; }
    TPrimaryKey GenerateNewId();
}