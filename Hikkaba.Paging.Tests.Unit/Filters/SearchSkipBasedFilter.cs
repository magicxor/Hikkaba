using Hikkaba.Paging.Models;

namespace Hikkaba.Paging.Tests.Unit.Filters;

public class SearchSkipBasedFilter : SkipBasedPagingFilter
{
    public string? SearchText { get; set; }
}
