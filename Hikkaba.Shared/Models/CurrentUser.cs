using System.Collections.Generic;

namespace Hikkaba.Shared.Models;

public class CurrentUser
{
    public required int Id { get; init; }
    public required string UserName { get; init; }
    public required HashSet<string> Roles { get; init; }
    public required HashSet<int> ModeratedCategories { get; init; }
}
