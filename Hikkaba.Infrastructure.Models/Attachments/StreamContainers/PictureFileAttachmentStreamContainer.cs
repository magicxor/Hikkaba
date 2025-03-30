namespace Hikkaba.Infrastructure.Models.Attachments.StreamContainers;

public class PictureFileAttachmentStreamContainer : FileAttachmentStreamContainer
{
    public required int Width { get; set; }

    public required int Height { get; set; }

    public required ThumbnailStreamContainer? ThumbnailStreamContainer { get; set; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ThumbnailStreamContainer?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (ThumbnailStreamContainer != null)
            await ThumbnailStreamContainer.DisposeAsync();

        await base.DisposeAsyncCore();
    }
}
