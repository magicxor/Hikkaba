using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Shared.Constants;

namespace Hikkaba.Data.Entities;

[Table("Categories")]
public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    public bool IsDeleted { get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    [Required]
    [MinLength(Defaults.MinCategoryAliasLength)]
    [MaxLength(Defaults.MaxCategoryAliasLength)]
    public required string Alias { get; set; }

    [Required]
    [MinLength(Defaults.MinCategoryAndBoardNameLength)]
    [MaxLength(Defaults.MaxCategoryAndBoardNameLength)]
    public required string Name { get; set; }

    [Required]
    public bool IsHidden { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int DefaultBumpLimit { get; set; }

    [Required]
    public bool DefaultShowThreadLocalUserHash { get; set; }

    // FK id
    [ForeignKey(nameof(Board))]
    public int BoardId { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public int CreatedById { get; set; }

    [ForeignKey(nameof(ModifiedBy))]
    public int? ModifiedById { get; set; }

    // FK models
    [Required]
    public Board Board
    {
        get => _board
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Board));
        set => _board = value;
    }

    private Board? _board;

    [Required]
    public virtual ApplicationUser CreatedBy
    {
        get => _createdBy
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(CreatedBy));
        set => _createdBy = value;
    }

    private ApplicationUser? _createdBy;

    public virtual ApplicationUser? ModifiedBy { get; set; }

    // Relations
    public virtual ICollection<Thread> Threads { get; set; } = new List<Thread>();
    public virtual ICollection<CategoryToModerator> Moderators { get; set; } = new List<CategoryToModerator>();
}
