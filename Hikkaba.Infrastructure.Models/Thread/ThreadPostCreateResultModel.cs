namespace Hikkaba.Infrastructure.Models.Thread;

public class ThreadPostCreateResultModel
{
    public required long ThreadId { get; init; }
    public required long PostId { get; init; }
    public required IReadOnlyList<Guid> DeletedBlobContainerIds { get; init; }
}
