using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Common.Constants;

namespace Hikkaba.Data.Entities;

[Table("Boards")]
public class Board
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MinLength(Defaults.MinCategoryAndBoardNameLength)]
    [MaxLength(Defaults.MaxCategoryAndBoardNameLength)]
    public required string Name { get; set; }

    // Relations
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
