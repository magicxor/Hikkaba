namespace Hikkaba.Infrastructure.Models.Administration;

public sealed class DashboardModel
{
    public required IReadOnlyList<CategoryModeratorsModel> CategoriesModerators { get; set; }
}
