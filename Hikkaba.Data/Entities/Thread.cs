using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Shared.Constants;

namespace Hikkaba.Data.Entities;

[Table("Threads")]
public class Thread
{
    [Key]
    public long Id { get; set; }

    [Required]
    public bool IsDeleted { get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    [Required]
    [MinLength(Defaults.MinTitleLength)]
    [MaxLength(Defaults.MaxTitleLength)]
    public required string Title { get; set; }

    [Required]
    public bool IsPinned { get; set; }

    [Required]
    public bool IsClosed { get; set; }

    [Required]
    public bool IsCyclic { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int BumpLimit { get; set; }

    [Required]
    public required Guid Salt { get; set; }

    // FK id
    [ForeignKey(nameof(Category))]
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public int? CreatedById { get; set; }

    [ForeignKey(nameof(ModifiedBy))]
    public int? ModifiedById { get; set; }

    // FK models
    [Required]
    public virtual Category Category
    {
        get => _category
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Category));
        set => _category = value;
    }

    private Category? _category;

    public virtual ApplicationUser? CreatedBy { get; set; }

    public virtual ApplicationUser? ModifiedBy { get; set; }

    // Relations
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
