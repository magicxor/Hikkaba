using TPrimaryKey = System.Guid;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Common.Constants;

namespace Hikkaba.Web.ViewModels.PostsViewModels
{
    public class PostEditViewModel
    {
        [Required]
        public TPrimaryKey Id { get; set; }

        [DataType(DataType.MultilineText)]
        [MaxLength(Defaults.MaxMessageLength)]
        [Display(Name = @"Message")]
        public string Message { get; set; }

        [Required]
        public string CategoryAlias { get; set; }
        public TPrimaryKey ThreadId { get; set; }
    }
}
