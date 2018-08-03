using System.ComponentModel.DataAnnotations;
using Hikkaba.Common.Constants;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.ViewModels.ThreadsViewModels
{
    public class ThreadAnonymousCreateViewModel
    {
        [MinLength(Defaults.MinTitleLength)]
        [MaxLength(Defaults.MaxTitleLength)]
        [Required]
        [Display(Name = @"Title")]
        public string Title { get; set; }

        [DataType(DataType.MultilineText)]
        [MinLength(Defaults.MinMessageLength)]
        [MaxLength(Defaults.MaxMessageLength)]
        [Required]
        [Display(Name = @"Message")]
        public string Message { get; set; }

        [DataType(DataType.Upload)]
        [Required]
        [Display(Name = @"Attachments")]
        public IFormFileCollection Attachments { get; set; }

        [Required]
        public string CategoryAlias { get; set; }
        public string CategoryName { get; set; }

        [Range(minimum: 0, maximum: Defaults.MaxAttachmentsCount, ErrorMessage = "Maximum {2} attachments allowed")]
        public int AttachmentsCount => Attachments?.Count ?? 0;
    }
}