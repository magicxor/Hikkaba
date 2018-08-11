using System.ComponentModel.DataAnnotations.Schema;
using TPrimaryKey = System.Guid;

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
