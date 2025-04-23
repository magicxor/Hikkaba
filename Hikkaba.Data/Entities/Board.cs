using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Shared.Constants;

namespace Hikkaba.Data.Entities;

[Table("Boards")]
public class Board
{
    [Key]
    public int Id { get; set; }

    // Relations
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
