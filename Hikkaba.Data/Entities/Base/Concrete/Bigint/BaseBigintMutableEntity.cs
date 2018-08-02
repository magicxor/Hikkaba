using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Entities.Base.Generic;

namespace Hikkaba.Data.Entities.Base.Concrete.Bigint
{
    public interface IBaseBigintMutableEntity: IBaseMutableEntity<long> { }
    public abstract class BaseBigintMutableEntity : BaseMutableEntity<long>, IBaseBigintMutableEntity
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