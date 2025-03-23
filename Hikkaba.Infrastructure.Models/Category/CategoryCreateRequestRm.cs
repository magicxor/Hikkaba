using Hikkaba.Infrastructure.Models.ApplicationUser;
using Hikkaba.Infrastructure.Models.Thread;

namespace Hikkaba.Infrastructure.Models.Category;

public class CategoryCreateRequestRm
{
    public required string Alias { get; set; }
    public required string Name { get; set; }
    public required bool IsHidden { get; set; }
    public required int DefaultBumpLimit { get; set; }
    public required bool DefaultShowThreadLocalUserHash { get; set; }
}
