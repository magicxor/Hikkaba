using System;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Common.Constants;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.ViewModels.ThreadsViewModels
{
    public class ThreadAnonymousCreateViewModel
    {
        [MinLength(3)]
        [MaxLength(100)]
        [Required]
        [Display(Name = @"Title")]
        public string Title { get; set; }

        [DataType(DataType.MultilineText)]
        [MinLength(4)]
        [MaxLength(4000)]
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