using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.ViewModels.PostsViewModels
{
    public class PostAnonymousCreateViewModel
    {
        [Required]
        [Display(Name = @"Sage")]
        public bool IsSageEnabled { get; set; }

        [DataType(DataType.MultilineText)]
        [MinLength(4)]
        [MaxLength(4000)]
        [Required]
        [Display(Name = @"Message")]
        public string Message { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = @"Attachments")]
        public IFormFileCollection Attachments { get; set; }

        [Required]
        public Guid ThreadId { get; set; }
        public string CategoryAlias { get; set; }
        public string CategoryName { get; set; }
    }
}