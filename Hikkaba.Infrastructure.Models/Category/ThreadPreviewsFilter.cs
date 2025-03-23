using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.Category;

public class ThreadPreviewsFilter : PageBasedPagingFilter
{
    public required string CategoryAlias { get; set; }
    public bool IncludeDeleted { get; set; }
}
