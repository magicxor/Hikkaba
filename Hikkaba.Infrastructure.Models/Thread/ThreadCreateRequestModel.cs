namespace Hikkaba.Infrastructure.Models.Thread;

public class ThreadCreateRequestModel
{
    public required string CategoryAlias { get; set; }

    public required string ThreadTitle { get; set; }

    public required Guid BlobContainerId { get; set; }

    public required string MessageHtml { get; set; }

    public required string MessageText { get; set; }

    public required byte[]? UserIpAddress { get; set; }

    public required string UserAgent { get; set; }
}
