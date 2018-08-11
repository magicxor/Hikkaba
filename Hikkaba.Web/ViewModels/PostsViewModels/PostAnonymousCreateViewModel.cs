using System.ComponentModel.DataAnnotations;
using Hikkaba.Common.Constants;
using Microsoft.AspNetCore.Http;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Web.ViewModels.PostsViewModels
{
    public class PostAnonymousCreateViewModel
    {
        [Required]
        [Display(Name = @"Sage")]
        public bool IsSageEnabled { get; set; }

        [DataType(DataType.MultilineText)]
        [MinLength(Defaults.MinMessageLength)]
        [MaxLength(Defaults.MaxMessageLength)]
        [Required]
        [Display(Name = @"Message")]
        public string Message { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = @"Attachments")]
        public IFormFileCollection Attachments { get; set; }

        [Required]
        public TPrimaryKey ThreadId { get; set; }
        public string CategoryAlias { get; set; }
        public string CategoryName { get; set; }

        [Range(minimum: 0, maximum: Defaults.MaxAttachmentsCount, ErrorMessage = "Maximum {2} attachments allowed")]
        public int AttachmentsCount => Attachments?.Count ?? 0;
    }
}