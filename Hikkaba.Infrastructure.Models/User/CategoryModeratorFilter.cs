using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.User;

public sealed class CategoryModeratorFilter : SortingFilter
{
    public required bool IncludeDeleted { get; set; }
    public required string CategoryAlias { get; set; }
}
