namespace Hikkaba.Infrastructure.Models.Attachments.StreamContainers;

public sealed class FileAttachmentContainerCollection : List<FileAttachmentStreamContainer>, IDisposable, IAsyncDisposable
{
    public FileAttachmentContainerCollection()
    {
    }

    public FileAttachmentContainerCollection(IEnumerable<FileAttachmentStreamContainer> collection)
        : base(collection)
    {
    }

    public FileAttachmentContainerCollection(int capacity)
        : base(capacity)
    {
    }

    public void Dispose()
    {
        foreach (var fileAttachmentSm in this)
        {
            fileAttachmentSm.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var fileAttachmentSm in this)
        {
            await fileAttachmentSm.DisposeAsync();
        }
    }
}
