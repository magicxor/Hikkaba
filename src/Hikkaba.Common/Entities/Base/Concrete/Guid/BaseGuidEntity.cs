using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Common.Entities.Base
{
    public interface IBaseGuidEntity: IBaseEntity<Guid> { }
    public abstract class BaseGuidEntity: BaseEntity<Guid>, IBaseGuidEntity
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
