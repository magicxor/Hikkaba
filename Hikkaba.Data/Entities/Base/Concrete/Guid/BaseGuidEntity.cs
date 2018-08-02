using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Entities.Base.Generic;

namespace Hikkaba.Data.Entities.Base.Concrete.Guid
{
    public interface IBaseGuidEntity: IBaseEntity<System.Guid> { }
    public abstract class BaseGuidEntity: BaseEntity<System.Guid>, IBaseGuidEntity
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override System.Guid Id { get; set; } = System.Guid.NewGuid();

        public override System.Guid GenerateNewId()
        {
            return System.Guid.NewGuid();
        }
    }
}
