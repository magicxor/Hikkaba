namespace Hikkaba.Infrastructure.Models.Attachments;

public sealed class ThumbnailRm : IDisposable, IAsyncDisposable
{
    public required int Width { get; set; }
    public required int Height { get; set; }
    public required Stream ContentStream { get; set; }

    public void Dispose()
    {
        ContentStream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await ContentStream.DisposeAsync();
    }
}
