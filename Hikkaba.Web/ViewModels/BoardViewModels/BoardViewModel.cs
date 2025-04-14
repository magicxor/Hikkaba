using Hikkaba.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.BoardViewModels;

public sealed class BoardViewModel
{
    [Required]
    public required int Id { get; set; }

    [Required]
    [MinLength(Defaults.MinCategoryAndBoardNameLength)]
    [MaxLength(Defaults.MaxCategoryAndBoardNameLength)]
    public required string Name { get; set; }
}
