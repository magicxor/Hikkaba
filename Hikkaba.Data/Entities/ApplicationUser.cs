using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Contracts;
using Hikkaba.Data.Entities.Attachments;
using Microsoft.AspNetCore.Identity;

namespace Hikkaba.Data.Entities;

public class ApplicationUser : IdentityUser<int>, IHasCreatedAt
{
    [Required]
    public bool IsDeleted { get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    // Relations
    [InverseProperty(nameof(Ban.CreatedBy))]
    public virtual ICollection<Ban> CreatedBans { get; set; } = new List<Ban>();

    [InverseProperty(nameof(Category.CreatedBy))]
    public virtual ICollection<Category> CreatedCategories { get; set; } = new List<Category>();

    [InverseProperty(nameof(Notice.CreatedBy))]
    public virtual ICollection<Notice> CreatedNotices { get; set; } = new List<Notice>();

    [InverseProperty(nameof(Ban.ModifiedBy))]
    public virtual ICollection<Ban> ModifiedBans { get; set; } = new List<Ban>();

    [InverseProperty(nameof(Category.ModifiedBy))]
    public virtual ICollection<Category> ModifiedCategories { get; set; } = new List<Category>();

    [InverseProperty(nameof(Post.ModifiedBy))]
    public virtual ICollection<Post> ModifiedPosts { get; set; } = new List<Post>();

    public virtual ICollection<CategoryToModerator> ModerationCategories { get; set; } = new List<CategoryToModerator>();
}
