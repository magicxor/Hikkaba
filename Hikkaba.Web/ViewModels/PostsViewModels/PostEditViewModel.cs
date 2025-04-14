using System.ComponentModel.DataAnnotations;
using Hikkaba.Shared.Constants;

namespace Hikkaba.Web.ViewModels.PostsViewModels;

public sealed class PostEditViewModel
{
    [Required]
    public required long Id { get; set; }

    [DataType(DataType.MultilineText)]
    [MaxLength(Defaults.MaxMessageLength)]
    [Display(Name = @"Message")]
    public required string Message { get; set; }

    [Required]
    public required string CategoryAlias { get; set; }
    public required long ThreadId { get; set; }
}
