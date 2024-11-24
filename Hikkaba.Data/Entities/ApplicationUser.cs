using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Common.Attributes;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Data.Entities.Base.Current;
using Microsoft.AspNetCore.Identity;

namespace Hikkaba.Data.Entities;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser<TPrimaryKey>, IBaseEntity
{
    public bool IsDeleted { get; set; }

    [DateTimeKind(DateTimeKind.Utc)]
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

    public TPrimaryKey GenerateNewId()
    {
        return KeyUtils.GenerateNew();
    }
}
