using System.IO;

namespace Hikkaba.Infrastructure.Extensions;

public static class StreamExtensions
{
    public static byte[] CopyToByteArray(this Stream input)
    {
        input.Position = 0;
        using (MemoryStream ms = new MemoryStream())
        {
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
}