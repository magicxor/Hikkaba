using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.Category;

public class CategoryModeratorsFilter : SortingFilter
{
    public required int CategoryId { get; set; }
}
