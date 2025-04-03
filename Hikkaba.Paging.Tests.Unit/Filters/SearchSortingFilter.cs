using Hikkaba.Paging.Models;

namespace Hikkaba.Paging.Tests.Unit.Filters;

public class SearchSortingFilter : SortingFilter
{
    public string? SearchText { get; set; }
}
