using System.ComponentModel.DataAnnotations;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.DataAnnotations;
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
    [MaxLength(Defaults.MaxMessageHtmlLength)]
    [Display(Name = @"Message")]
    public required string Message { get; set; }

    [Display(Name = @"Attachments")]
    [AllowedExtensions(Defaults.AllAllowedExtensions)]
    [MaxFileSize(Defaults.MaxAttachmentSize)]
    [MaxFileCollectionSize(Defaults.MaxAttachmentsTotalSize)]
    [MaxFileCount(Defaults.MaxAttachmentsCount)]
    public IFormFileCollection? Attachments { get; set; } = new FormFileCollection();

    [Required]
    public required long ThreadId { get; set; }

    [Required]
    [MaxLength(Defaults.MaxCategoryAliasLength)]
    public required string CategoryAlias { get; set; }

    [MaxLength(Defaults.MaxCategoryAndBoardNameLength)]
    public string? CategoryName { get; set; }
}
