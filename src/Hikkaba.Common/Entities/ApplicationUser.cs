using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Common.Entities.Attachments;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Hikkaba.Common.Entities
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser<Guid>
    {
        public bool IsDeleted { get; set; }
        public DateTime? LastLogin { get; set; }

        [InverseProperty("CreatedBy")]
        public virtual ICollection<Ban> CreatedBans { get; set; }
        [InverseProperty("CreatedBy")]
        public virtual ICollection<Category> CreatedCategories { get; set; }
        [InverseProperty("CreatedBy")]
        public virtual ICollection<Post> CreatedPosts { get; set; }
        public virtual ICollection<Notice> CreatedNotices { get; set; }

        [InverseProperty("ModifiedBy")]
        public virtual ICollection<Ban> ModifiedBans { get; set; }
        [InverseProperty("ModifiedBy")]
        public virtual ICollection<Category> ModifiedCategories { get; set; }
        [InverseProperty("ModifiedBy")]
        public virtual ICollection<Post> ModifiedPosts { get; set; }

        public virtual ICollection<CategoryToModerator> ModerationCategories { get; set; }
    }
}
