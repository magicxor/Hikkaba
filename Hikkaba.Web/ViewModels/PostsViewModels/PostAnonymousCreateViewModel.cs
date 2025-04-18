using System.ComponentModel.DataAnnotations;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.ViewModels.PostsViewModels;

[AtLeastOneProperty(nameof(Attachments), nameof(Message), ErrorMessage = "Message or attachment is required.")]
public sealed class PostAnonymousCreateViewModel
{
    [Required]
    [Display(Name = @"Sage")]
    public required bool IsSageEnabled { get; set; }

    [DataType(DataType.MultilineText)]
    [MaxLength(Defaults.MaxMessageHtmlLength)]
    [Display(Name = @"Message")]
    public string? Message { get; set; } = string.Empty;

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
