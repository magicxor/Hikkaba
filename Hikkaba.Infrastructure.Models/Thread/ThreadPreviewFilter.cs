using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.Thread;

public sealed class ThreadPreviewFilter : PageBasedPagingFilter
{
    public required string CategoryAlias { get; set; }
    public bool IncludeDeleted { get; set; }
}
