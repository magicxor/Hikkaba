namespace Hikkaba.Infrastructure.Models.Administration;

public class DashboardModel
{
    public required IReadOnlyList<CategoryModeratorsModel> CategoriesModerators { get; set; }
}
