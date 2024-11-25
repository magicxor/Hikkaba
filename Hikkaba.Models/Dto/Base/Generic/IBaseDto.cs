namespace Hikkaba.Models.Dto.Base.Generic;

public interface IBaseDto<TPrimaryKey>
{
    TPrimaryKey Id { get; set; }
}