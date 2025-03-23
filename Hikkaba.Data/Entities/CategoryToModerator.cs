using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hikkaba.Data.Entities;

[Table("CategoriesToModerators")]
public class CategoryToModerator
{
    [Key]
    public int Id { get; set; }

    // FK id
    [ForeignKey(nameof(Category))]
    public int CategoryId { get; set; }

    [ForeignKey(nameof(Moderator))]
    public int ModeratorId { get; set; }

    // FK models
    [Required]
    public Category Category
    {
        get => _category
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Category));
        set => _category = value;
    }

    private Category? _category;

    [Required]
    public ApplicationUser Moderator
    {
        get => _moderator
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Moderator));
        set => _moderator = value;
    }

    private ApplicationUser? _moderator;
}
