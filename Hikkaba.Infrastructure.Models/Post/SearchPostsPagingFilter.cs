using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.Post;

public sealed class SearchPostsPagingFilter : PageBasedPagingFilter
{
    public required string SearchQuery { get; set; }
}
