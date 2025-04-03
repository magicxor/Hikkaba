using Hikkaba.Paging.Models;

namespace Hikkaba.Paging.Tests.Unit.Filters;

public sealed class SearchPageBasedFilter : PageBasedPagingFilter
{
    public string? SearchText { get; set; }
}
