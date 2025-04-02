using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.Post;

public class ThreadPostsFilter : SortingFilter
{
    public required long ThreadId { get; set; }
    public long? PostId { get; set; }
    public bool IncludeDeleted { get; set; }
}
