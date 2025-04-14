using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.Post;

public sealed class PostPagingFilter : PageBasedPagingFilter
{
    public bool IncludeHidden { get; set; }
    public bool IncludeDeleted { get; set; }
    public bool IncludeTotalCount { get; set; }
}
