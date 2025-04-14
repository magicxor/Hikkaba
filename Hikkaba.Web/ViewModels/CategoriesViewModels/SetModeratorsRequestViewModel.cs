using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace Hikkaba.Web.ViewModels.CategoriesViewModels;

public sealed class SetModeratorsRequestViewModel
{
    [Required]
    public required IReadOnlyList<int> ModeratorIds { get; set; }
}
