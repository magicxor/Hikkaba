using Hikkaba.Shared.Enums;

namespace Hikkaba.Infrastructure.Models.Attachments.StreamContainers;

public class FileAttachmentStreamContainer : IDisposable, IAsyncDisposable
{
    public required Guid BlobId { get; set; }

    public required AttachmentType AttachmentType { get; set; }

    public required string FileNameWithoutExtension { get; set; }

    public required string FileExtension { get; set; }

    public required string FileNameWithExtension { get; set; }

    public required long FileSize { get; set; }

    public required string ContentType { get; set; }

    public required byte[] FileHash { get; set; }

    public required FileStream FileStream { get; set; }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            FileStream.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        await FileStream.DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
}
