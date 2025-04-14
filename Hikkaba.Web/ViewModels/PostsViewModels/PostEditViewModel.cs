using System.ComponentModel.DataAnnotations;
using Hikkaba.Shared.Constants;

namespace Hikkaba.Web.ViewModels.PostsViewModels;

public sealed class PostEditViewModel
{
    [Required]
    public required long Id { get; set; }

    [DataType(DataType.MultilineText)]
    [MaxLength(Defaults.MaxMessageHtmlLength)]
    [Display(Name = @"Message")]
    public required string MessageOriginalMarkup { get; set; }

    [Required]
    public required string CategoryAlias { get; set; }

    [Required]
    public required long ThreadId { get; set; }
}
