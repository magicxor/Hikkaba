using System.ComponentModel.DataAnnotations;
using Hikkaba.Common.Constants;

namespace Hikkaba.Web.ViewModels.CategoriesViewModels
{
    public class CategoryCreateViewModel
    {
        [Required]
        [RegularExpression("^[A-Za-z]+$")]
        [MinLength(Defaults.MinCategoryAliasLength)]
        [MaxLength(Defaults.MaxCategoryAliasLength)]
        [Display(Name = @"Alias")]
        public string Alias { get; set; }

        [Display(Name = @"Name")]
        public string Name { get; set; }

        [Display(Name = @"Is hidden")]
        public bool IsHidden { get; set; }

        [Display(Name = @"Default bump limit")]
        public int DefaultBumpLimit { get; set; } = Defaults.MinBumpLimit;

        [Display(Name = @"Show thread-local user hash by default")]
        public bool DefaultShowThreadLocalUserHash { get; set; }
    }
}