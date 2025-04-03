using Hikkaba.Paging.Models;

namespace Hikkaba.Paging.Tests.Unit.Filters;

public class SearchPageBasedFilter : PageBasedPagingFilter
{
    public string? SearchText { get; set; }
}
