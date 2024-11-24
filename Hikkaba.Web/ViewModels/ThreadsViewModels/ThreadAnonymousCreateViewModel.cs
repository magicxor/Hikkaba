using System.ComponentModel.DataAnnotations;
using Hikkaba.Common.Constants;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.ViewModels.ThreadsViewModels;

public class ThreadAnonymousCreateViewModel
{
    [Required]
    [MinLength(Defaults.MinTitleLength)]
    [MaxLength(Defaults.MaxTitleLength)]
    [Display(Name = @"Title")]
    public string Title { get; set; }

    [Required]
    [DataType(DataType.MultilineText)]
    [MinLength(Defaults.MinMessageLength)]
    [MaxLength(Defaults.MaxMessageLength)]
    [Display(Name = @"Message")]
    public string Message { get; set; }

    [Required]
    [DataType(DataType.Upload)]
    [Display(Name = @"Attachments")]
    public IFormFileCollection Attachments { get; set; }

    [Required]
    public string CategoryAlias { get; set; }
    public string CategoryName { get; set; }

    [Range(minimum: 0, maximum: Defaults.MaxAttachmentsCount, ErrorMessage = "Maximum {2} attachments allowed")]
    public int AttachmentsCount => Attachments?.Count ?? 0;
}