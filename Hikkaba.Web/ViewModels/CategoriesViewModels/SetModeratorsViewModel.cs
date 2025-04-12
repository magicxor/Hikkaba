using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hikkaba.Web.ViewModels.CategoriesViewModels;

public class SetModeratorsViewModel
{
    public required string CategoryAlias { get; set; }
    public required IReadOnlyList<int> ModeratorIds { get; set; }
    public required IReadOnlyList<SelectListItem> Users { get; set; }
}
