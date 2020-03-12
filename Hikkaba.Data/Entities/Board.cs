using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Entities.Base.Current;

namespace Hikkaba.Data.Entities
{
    [Table("Boards")]
    public class Board: BaseEntity
    {
        [Required]
        [MinLength(Defaults.MinCategoryAndBoardNameLength)]
        [MaxLength(Defaults.MaxCategoryAndBoardNameLength)]
        public string Name { get; set; }

        public virtual ICollection<Category> Categories { get; set; }
    }
}
