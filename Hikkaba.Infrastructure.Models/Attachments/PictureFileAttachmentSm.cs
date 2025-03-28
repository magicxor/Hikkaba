namespace Hikkaba.Infrastructure.Models.Attachments;

public class PictureFileAttachmentSm : GenericFileAttachmentSm
{
    public required int Width { get; set; }

    public required int Height { get; set; }

    public required ThumbnailRm? ThumbnailRm { get; set; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ThumbnailRm?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (ThumbnailRm != null)
            await ThumbnailRm.DisposeAsync();

        await base.DisposeAsyncCore();
    }
}
