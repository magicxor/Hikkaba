
namespace Hikkaba.Infrastructure.Models.Thread;

public class ThreadPostCreateResultModel
{
    public required long ThreadId { get; set; }
    public required long PostId { get; set; }
    public required List<Guid> DeletedBlobContainerIds { get; set; }
}
