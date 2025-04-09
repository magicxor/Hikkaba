using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.Thread;

public class CategoryThreadFilter
{
    public required string CategoryAlias { get; set; }
    public required long ThreadId { get; set; }
    public bool IncludeDeleted { get; set; }
}
