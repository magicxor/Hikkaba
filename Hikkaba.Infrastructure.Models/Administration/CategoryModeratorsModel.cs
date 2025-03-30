using Hikkaba.Infrastructure.Models.ApplicationUser;

namespace Hikkaba.Infrastructure.Models.Administration;

public class CategoryModeratorsModel
{
    public required CategoryDashboardModel Category { get; set; }
    public required IReadOnlyList<ApplicationUserPreviewModel> Moderators { get; set; }
}
