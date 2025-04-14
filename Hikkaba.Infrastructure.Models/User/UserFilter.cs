using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.User;

public sealed class UserFilter : SortingFilter
{
    public required bool IncludeDeleted { get; set; }
}
