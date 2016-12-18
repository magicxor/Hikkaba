using System;
using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.PostsViewModels
{
    public class PostEditViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [DataType(DataType.MultilineText)]
        [MaxLength(4000)]
        [Display(Name = @"Message")]
        public string Message { get; set; }
    }
}
