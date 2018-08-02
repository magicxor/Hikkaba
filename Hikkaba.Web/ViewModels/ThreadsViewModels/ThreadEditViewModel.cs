using System;
using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.ThreadsViewModels
{
    public class ThreadEditViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [MinLength(3)]
        [MaxLength(100)]
        [Required]
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
        public int BumpLimit { get; set; }

        [Required]
        [Display(Name = @"Show thread-local user hash")]
        public bool ShowThreadLocalUserHash { get; set; }
        
        [Required]
        [Display(Name = @"Category alias")]
        public string CategoryAlias { get; set; }
    }
}
