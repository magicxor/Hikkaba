namespace Hikkaba.Infrastructure.Models.Thread;

public sealed class CategoryThreadModel
{
    public required long ThreadId { get; set; }
    public required string CategoryAlias { get; set; }
    public required string CategoryName { get; set; }
}
