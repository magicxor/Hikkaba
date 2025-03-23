using Hikkaba.Infrastructure.Models.ApplicationUser;
using Hikkaba.Infrastructure.Models.Category;

namespace Hikkaba.Infrastructure.Models.Administration;

public class CategoryModeratorsRm
{
    public required CategoryDashboardViewRm Category { get; set; }
    public required IReadOnlyList<ApplicationUserPreviewRm> Moderators { get; set; }
}
