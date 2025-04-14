using System.Text;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
internal static partial class StringBytesMapper
{
    [UserMapping]
    public static byte[] StringToBytes(string src) => Encoding.UTF8.GetBytes(src);

    [UserMapping]
    public static string BytesToString(byte[] src) => Encoding.UTF8.GetString(src);
}
