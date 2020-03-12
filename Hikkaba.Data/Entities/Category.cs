using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Entities.Base.Current;

namespace Hikkaba.Data.Entities
{
    [Table("Categories")]
    public class Category: BaseMutableEntity
    { 
        [Required]
        [MinLength(Defaults.MinCategoryAliasLength)]
        [MaxLength(Defaults.MaxCategoryAliasLength)]
        public string Alias { get; set; }

        [Required]
        [MinLength(Defaults.MinCategoryAndBoardNameLength)]
        [MaxLength(Defaults.MaxCategoryAndBoardNameLength)]
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
