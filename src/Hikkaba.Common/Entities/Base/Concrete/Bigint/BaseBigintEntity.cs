using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Common.Entities.Base
{
    public interface IBaseBigintEntity: IBaseEntity<long> { }
    public abstract class BaseBigintEntity : BaseEntity<long>, IBaseBigintEntity
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }

        public override long GenerateNewId()
        {
            return 0;
        }
    }
}
