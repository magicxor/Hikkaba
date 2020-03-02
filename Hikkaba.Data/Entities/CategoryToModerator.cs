using TPrimaryKey = System.Guid;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hikkaba.Data.Entities
{
    [Table("CategoriesToModerators")]
    public class CategoryToModerator
    {
        public TPrimaryKey CategoryId { get; set; }
        public TPrimaryKey ApplicationUserId { get; set; }

        public virtual Category Category { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
