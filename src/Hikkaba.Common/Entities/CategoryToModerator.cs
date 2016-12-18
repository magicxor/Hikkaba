using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Hikkaba.Common.Entities
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
