using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.Post;

public class PostPagingFilter : PageBasedPagingFilter
{
    public bool IncludeHidden { get; set; }
    public bool IncludeDeleted { get; set; }
    public bool IncludeTotalCount { get; set; }
}
