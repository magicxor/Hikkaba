namespace Hikkaba.Infrastructure.Models.Attachments;

public sealed class TempFileStreamCollection : List<FileStream>, IDisposable, IAsyncDisposable
{
    public void Dispose()
    {
        foreach (var tempFileStream in this)
        {
            tempFileStream.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var tempFileStream in this)
        {
            await tempFileStream.DisposeAsync();
        }
    }
}
