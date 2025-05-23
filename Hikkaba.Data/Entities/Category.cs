﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Contracts;
using Hikkaba.Shared.Constants;

namespace Hikkaba.Data.Entities;

[Table("Categories")]
public class Category : IHasAuditColumns
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
    public required int DefaultBumpLimit { get; set; }

    [Required]
    public bool ShowThreadLocalUserHash { get; set; }

    [Required]
    public bool ShowCountry { get; set; }

    [Required]
    public bool ShowOs { get; set; }

    [Required]
    public bool ShowBrowser { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public required int MaxThreadCount { get; set; }

    // FK id
    [ForeignKey(nameof(CreatedBy))]
    public int CreatedById { get; set; }

    [ForeignKey(nameof(ModifiedBy))]
    public int? ModifiedById { get; set; }

    // FK models
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
