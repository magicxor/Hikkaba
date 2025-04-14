using Hikkaba.Paging.Models;

namespace Hikkaba.Paging.Tests.Unit.Filters;

public sealed class SearchSortingFilter : SortingFilter
{
    public string? SearchText { get; set; }
}
