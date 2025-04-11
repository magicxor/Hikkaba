using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.Category;

public sealed class CategoryFilter : SortingFilter
{
    public bool IncludeHidden { get; set; }
    public bool IncludeDeleted { get; set; }
}
