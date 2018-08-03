using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Entities.Base.Concrete.Guid;

namespace Hikkaba.Data.Entities.Base.Current
{
    public interface IBaseMutableEntity: IBaseGuidMutableEntity { }
    public abstract class BaseMutableEntity : BaseGuidMutableEntity, IBaseMutableEntity
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override Guid Id { get; set; } = Guid.NewGuid();

        public override Guid GenerateNewId()
        {
            return Guid.NewGuid();
        }
    }
}