using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Entities.Base.Current;

namespace Hikkaba.Data.Entities
{
    [Table("Categories")]
    public class Category: BaseMutableEntity
    { 
        [Required]
        [MaxLength(10)]
        public string Alias { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public bool IsHidden { get; set; }

        [Required]
        public int DefaultBumpLimit { get; set; }

        [Required]
        public bool DefaultShowThreadLocalUserHash { get; set; }

        [Required]
        public virtual Board Board { get; set; }

        public virtual ICollection<Thread> Threads { get; set; }
        public virtual ICollection<CategoryToModerator> Moderators { get; set; }
    }
}
