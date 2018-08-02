using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hikkaba.Data.Entities
{
    [Table("CategoriesToModerators")]
    public class CategoryToModerator
    {
        public Guid CategoryId { get; set; }
        public Guid ApplicationUserId { get; set; }

        public virtual Category Category { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
