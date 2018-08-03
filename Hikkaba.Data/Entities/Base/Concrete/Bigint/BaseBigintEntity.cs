using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Entities.Base.Generic;

namespace Hikkaba.Data.Entities.Base.Concrete.Bigint
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
