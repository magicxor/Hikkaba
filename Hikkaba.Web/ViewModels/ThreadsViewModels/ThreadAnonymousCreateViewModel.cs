using System.ComponentModel.DataAnnotations;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.ViewModels.ThreadsViewModels;

public class ThreadAnonymousCreateViewModel
{
    [MinLength(Defaults.MinTitleLength)]
    [MaxLength(Defaults.MaxTitleLength)]
    [Display(Name = @"Title")]
    public required string? Title { get; set; }

    [Required]
    [DataType(DataType.MultilineText)]
    [MinLength(1)]
    [MaxLength(Defaults.MaxMessageHtmlLength)]
    [Display(Name = @"Message")]
    public required string Message { get; set; }

    [Required]
    [Display(Name = @"Attachments")]
    [AllowedExtensions(Defaults.AllAllowedExtensions)]
    [MaxFileSize(Defaults.MaxAttachmentSize)]
    [MaxFileCollectionSize(Defaults.MaxAttachmentsTotalSize)]
    [MinFileCount(1)]
    [MaxFileCount(Defaults.MaxAttachmentsCount)]
    public required IFormFileCollection Attachments { get; set; }

    [Required]
    [MaxLength(Defaults.MaxCategoryAliasLength)]
    public required string CategoryAlias { get; set; }

    [MaxLength(Defaults.MaxCategoryAndBoardNameLength)]
    public required string CategoryName { get; set; }
}
