namespace Hikkaba.Infrastructure.Models.Attachments;

public sealed class FileAttachmentCollection : List<GenericFileAttachmentSm>, IDisposable, IAsyncDisposable
{
    public FileAttachmentCollection()
    {
    }

    public FileAttachmentCollection(IEnumerable<GenericFileAttachmentSm> collection) : base(collection)
    {
    }

    public FileAttachmentCollection(int capacity) : base(capacity)
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
