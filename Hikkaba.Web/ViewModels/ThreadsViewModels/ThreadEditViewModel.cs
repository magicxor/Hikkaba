using TPrimaryKey = System.Guid;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Common.Constants;

namespace Hikkaba.Web.ViewModels.ThreadsViewModels
{
    public class ThreadEditViewModel
    {
        [Required]
        public TPrimaryKey Id { get; set; }

        [Required]
        [MinLength(Defaults.MinTitleLength)]
        [MaxLength(Defaults.MaxTitleLength)]
        [Display(Name = @"Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = @"Is pinned")]
        public bool IsPinned { get; set; }

        [Required]
        [Display(Name = @"Is closed")]
        public bool IsClosed { get; set; }

        [Required]
        [Display(Name = @"Bump limit")]
        [Range(Defaults.MinBumpLimit, Defaults.MaxBumpLimit)]
        public int BumpLimit { get; set; }

        [Required]
        [Display(Name = @"Show thread-local user hash")]
        public bool ShowThreadLocalUserHash { get; set; }
        
        [Required]
        [Display(Name = @"Category alias")]
        public string CategoryAlias { get; set; }
    }
}
