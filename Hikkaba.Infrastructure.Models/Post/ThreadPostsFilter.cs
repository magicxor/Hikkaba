using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.Post;

public sealed class ThreadPostsFilter : SortingFilter
{
    public long? PostId { get; set; }
    public required long ThreadId { get; set; }
    public bool IncludeDeleted { get; set; }
}
