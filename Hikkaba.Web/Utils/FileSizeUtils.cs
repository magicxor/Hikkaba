using System.Globalization;
using Humanizer;
using Humanizer.Bytes;

namespace Hikkaba.Web.Utils;

internal static class FileSizeUtils
{
    public static string Humanize(long sizeInBytes)
    {
        return ByteSize.FromBytes(sizeInBytes).Humanize("#.##", CultureInfo.InvariantCulture);
    }
}
