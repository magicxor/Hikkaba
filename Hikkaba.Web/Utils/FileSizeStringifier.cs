using Humanizer;
using Humanizer.Bytes;

namespace Hikkaba.Web.Utils;

public static class FileSizeStringifier
{
    public static string Stringify(long sizeInBytes)
    {
        return ByteSize.FromBytes(sizeInBytes).Humanize("#.##");
    }
}