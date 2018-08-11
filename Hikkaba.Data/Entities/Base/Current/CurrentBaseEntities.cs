using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Entities.Base.Generic;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Data.Entities.Base.Current
{
    public interface IBaseEntity : IBaseEntity<TPrimaryKey> { }
    public abstract class BaseEntity : BaseEntity<TPrimaryKey>, IBaseEntity
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override TPrimaryKey Id { get; set; } = System.Guid.NewGuid();

        public override TPrimaryKey GenerateNewId()
        {
            return System.Guid.NewGuid();
        }
    }

    public interface IBaseMutableEntity : IBaseMutableEntity<TPrimaryKey> { }
    public abstract class BaseMutableEntity : BaseMutableEntity<TPrimaryKey>, IBaseMutableEntity
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override TPrimaryKey Id { get; set; } = System.Guid.NewGuid();

        public override TPrimaryKey GenerateNewId()
        {
            return System.Guid.NewGuid();
        }
    }

    public abstract class BaseManyToManyEntity { }
}
