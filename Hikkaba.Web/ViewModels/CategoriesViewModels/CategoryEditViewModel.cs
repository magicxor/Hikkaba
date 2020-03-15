using TPrimaryKey = System.Guid;
using System;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Common.Constants;

namespace Hikkaba.Web.ViewModels.CategoriesViewModels
{
    public class CategoryEditViewModel: CategoryCreateViewModel
    {
        [Display(Name = @"Id")]
        public TPrimaryKey Id { get; set; }
    }
}
