using System.Collections.Generic;

namespace Hikkaba.Common.Models;

public class CurrentUser
{
    public required int Id { get; set; }
    public required string UserName { get; set; }
    public required HashSet<int> ModeratedCategories { get; set; }
}
