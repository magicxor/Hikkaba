using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.Thread;

public class ThreadPagingFilter : PageBasedPagingFilter
{
    public required bool IncludeDeleted { get; set; }
}
