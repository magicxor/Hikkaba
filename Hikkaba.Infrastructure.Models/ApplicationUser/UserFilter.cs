using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.ApplicationUser;

public sealed class UserFilter : SortingFilter
{
    public required bool IncludeDeleted { get; set; }
}
