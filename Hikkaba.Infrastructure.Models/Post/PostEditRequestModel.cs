namespace Hikkaba.Infrastructure.Models.Post;

public sealed class PostEditRequestModel
{
    public required long Id { get; set; }

    public required string MessageText { get; set; }

    public required string MessageHtml { get; set; }
}
