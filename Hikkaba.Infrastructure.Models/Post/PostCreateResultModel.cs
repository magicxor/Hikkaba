namespace Hikkaba.Infrastructure.Models.Post;

public class PostCreateResultModel
{
    public required long PostId { get; init; }
    public required IReadOnlyList<Guid> DeletedBlobContainerIds { get; init; }
}
