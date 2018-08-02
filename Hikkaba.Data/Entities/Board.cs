using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Entities.Base.Current;

namespace Hikkaba.Data.Entities
{
    [Table("Boards")]
    public class Board: BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public virtual ICollection<Category> Categories { get; set; }
    }
}
