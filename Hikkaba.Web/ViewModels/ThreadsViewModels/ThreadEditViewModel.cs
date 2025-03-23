using System.ComponentModel.DataAnnotations;
using Hikkaba.Common.Constants;

namespace Hikkaba.Web.ViewModels.ThreadsViewModels;

public class ThreadEditViewModel
{
    [Required]
    public required long Id { get; set; }

    [Required]
    [MinLength(Defaults.MinTitleLength)]
    [MaxLength(Defaults.MaxTitleLength)]
    [Display(Name = @"Title")]
    public required string Title { get; set; }

    [Required]
    [Display(Name = @"Is pinned")]
    public required bool IsPinned { get; set; }

    [Required]
    [Display(Name = @"Is closed")]
    public required bool IsClosed { get; set; }

    [Required]
    [Display(Name = @"Bump limit")]
    [Range(Defaults.MinBumpLimit, Defaults.MaxBumpLimit)]
    public required int BumpLimit { get; set; }

    [Required]
    [Display(Name = @"Show thread-local user hash")]
    public required bool ShowThreadLocalUserHash { get; set; }

    [Required]
    [Display(Name = @"Category alias")]
    public required string CategoryAlias { get; set; }
}
