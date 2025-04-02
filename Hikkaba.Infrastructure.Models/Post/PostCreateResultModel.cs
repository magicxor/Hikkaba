namespace Hikkaba.Infrastructure.Models.Post;

public class PostCreateResultModel
{
    public required long PostId { get; set; }
    public required List<Guid> DeletedBlobContainerIds { get; set; }
}
