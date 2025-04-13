using Hikkaba.Infrastructure.Models.User;

namespace Hikkaba.Infrastructure.Models.Administration;

public sealed class CategoryModeratorsModel
{
    public required CategoryDashboardModel Category { get; set; }
    public required IReadOnlyList<UserPreviewModel> Moderators { get; set; }
}
