namespace Hikkaba.Infrastructure.Models.Thread;

public sealed class ThreadEditRequestModel
{
    public required long Id { get; set; }
    public required string Title { get; set; }
    public required int BumpLimit { get; set; }
}
