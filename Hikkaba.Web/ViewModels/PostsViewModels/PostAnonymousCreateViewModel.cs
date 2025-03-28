using System.ComponentModel.DataAnnotations;
using Hikkaba.Common.Constants;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.ViewModels.PostsViewModels;

public class PostAnonymousCreateViewModel
{
    [Required]
    [Display(Name = @"Sage")]
    public required bool IsSageEnabled { get; set; }

    [Required]
    [DataType(DataType.MultilineText)]
    [MinLength(1)]
    [MaxLength(Defaults.MaxMessageLength)]
    [Display(Name = @"Message")]
    public required string Message { get; set; }

    [DataType(DataType.Upload)]
    [Display(Name = @"Attachments")]
    public IFormFileCollection Attachments { get; set; } = new FormFileCollection();

    [Required]
    public required long ThreadId { get; set; }
    public required string CategoryAlias { get; set; }
    public string? CategoryName { get; set; }

    [Range(minimum: 0, maximum: Defaults.MaxAttachmentsCount, ErrorMessage = "Maximum {2} attachments allowed")]
    public int AttachmentsCount => Attachments?.Count ?? 0;
}
