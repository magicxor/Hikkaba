using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Common.Entities.Base;

namespace Hikkaba.Common.Entities
{
    [Table("Threads")]
    public class Thread: BaseMutableEntity
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        public bool IsPinned { get; set; }

        [Required]
        public bool IsClosed { get; set; }

        [Required]
        public int BumpLimit { get; set; }

        [Required]
        public bool ShowThreadLocalUserHash { get; set; }
        
        [Required]
        public virtual Category Category { get; set; }

        public virtual ICollection<Post> Posts { get; set; }
    }
}
