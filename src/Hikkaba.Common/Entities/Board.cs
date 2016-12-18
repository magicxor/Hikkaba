using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Entities.Base;

namespace Hikkaba.Common.Entities
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
