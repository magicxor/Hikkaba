using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.ApplicationUser;

public sealed class CategoryModeratorFilter : SortingFilter
{
    public required bool IncludeDeleted { get; set; }
    public required string CategoryAlias { get; set; }
}
