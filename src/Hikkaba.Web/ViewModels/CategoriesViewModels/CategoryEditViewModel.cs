using System;
using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.CategoriesViewModels
{
    public class CategoryEditViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(10)]
        [Display(Name = @"Alias")]
        [RegularExpression(@"^[a-z0-9_\-\.]+$")]
        public string Alias { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = @"Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = @"Is hidden")]
        public bool IsHidden { get; set; }

        [Required]
        [Display(Name = @"Bump limit by default")]
        public int DefaultBumpLimit { get; set; }

        [Required]
        [Display(Name = @"Show thread-Local user hash by default")]
        public bool DefaultShowThreadLocalUserHash { get; set; }

        [Required]
        public Guid BoardId { get; set; }
    }
}