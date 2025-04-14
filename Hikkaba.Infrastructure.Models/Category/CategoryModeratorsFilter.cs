using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.Category;

public sealed class CategoryModeratorsFilter : SortingFilter
{
    public required int CategoryId { get; set; }
}
