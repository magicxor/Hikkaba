using System.ComponentModel.DataAnnotations;
using System.Linq;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.ViewModels.ThreadsViewModels;

public class ThreadAnonymousCreateViewModel
{
    [Required]
    [MinLength(Defaults.MinTitleLength)]
    [MaxLength(Defaults.MaxTitleLength)]
    [Display(Name = @"Title")]
    public required string Title { get; set; }

    [Required]
    [DataType(DataType.MultilineText)]
    [MinLength(1)]
    [MaxLength(Defaults.MaxMessageLength)]
    [Display(Name = @"Message")]
    public required string Message { get; set; }

    [Required]
    [Display(Name = @"Attachments")]
    [AllowedExtensions(Defaults.AllAllowedExtensions)]
    [FileSizeMax(Defaults.MaxAttachmentSize)]
    [FileCollectionSizeMax(Defaults.MaxAttachmentsTotalSize)]
    [FileMinCount(1)]
    [FileMaxCount(Defaults.MaxAttachmentsCount)]
    public required IFormFileCollection Attachments { get; set; }

    [Required]
    public required string CategoryAlias { get; set; }
    public required string CategoryName { get; set; }
}
