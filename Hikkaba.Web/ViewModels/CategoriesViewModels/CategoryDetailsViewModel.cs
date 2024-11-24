using System;
using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.CategoriesViewModels;

public class CategoryDetailsViewModel
{
    [Display(Name = @"Id")]
    public TPrimaryKey Id { get; set; }

    [Display(Name = @"Is deleted")]
    public bool IsDeleted { get; set; }

    [Display(Name = @"Creation date and time")]
    public DateTime Created { get; set; }

    [Display(Name = @"Modification date and time")]
    public DateTime? Modified { get; set; }

    [Display(Name = @"Alias")]
    public string Alias { get; set; }

    [Display(Name = @"Name")]
    public string Name { get; set; }

    [Display(Name = @"Is hidden")]
    public bool IsHidden { get; set; }

    [Display(Name = @"Default bump limit")]
    public int DefaultBumpLimit { get; set; }

    [Display(Name = @"Show thread-local user hash by default")]
    public bool DefaultShowThreadLocalUserHash { get; set; }

    [Display(Name = @"Board id")]
    public TPrimaryKey BoardId { get; set; }
}
