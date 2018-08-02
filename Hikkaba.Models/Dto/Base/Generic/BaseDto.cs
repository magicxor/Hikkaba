namespace Hikkaba.Models.Dto.Base.Generic
{
    public interface IBaseDto<TPrimaryKey>
    {
        TPrimaryKey Id { get; set; }
    }

    public abstract class BaseDto<TPrimaryKey> : IBaseDto<TPrimaryKey>
    {
        public TPrimaryKey Id { get; set; }
    }
}
