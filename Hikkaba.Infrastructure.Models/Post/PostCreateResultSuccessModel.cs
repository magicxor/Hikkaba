namespace Hikkaba.Infrastructure.Models.Post;

public sealed class PostCreateResultSuccessModel
{
    public required long PostId { get; init; }
    public required IReadOnlyList<Guid> DeletedBlobContainerIds { get; init; }
}
