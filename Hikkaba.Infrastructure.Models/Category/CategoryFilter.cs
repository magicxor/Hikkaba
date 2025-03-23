using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.Category;

public class CategoryFilter : SortingFilter
{
    public bool IncludeHidden { get; set; }
    public bool IncludeDeleted { get; set; }
}
