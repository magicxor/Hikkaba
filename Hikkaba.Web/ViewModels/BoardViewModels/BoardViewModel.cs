using TPrimaryKey = System.Guid;
using Hikkaba.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.BoardViewModels;

public class BoardViewModel
{
    [Required]
    public TPrimaryKey Id { get; set; }
        
    [Required]
    [MinLength(Defaults.MinCategoryAndBoardNameLength)]
    [MaxLength(Defaults.MaxCategoryAndBoardNameLength)]
    public string Name { get; set; }
}