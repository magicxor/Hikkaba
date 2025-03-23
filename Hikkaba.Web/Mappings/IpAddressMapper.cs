using System.Net;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
public static partial class IpAddressMapper
{
    [UserMapping]
    public static IPAddress StringToIpAddress(string src) => IPAddress.Parse(src);

    [UserMapping]
    public static IPAddress BytesToIpAddress(byte[] src) => new(src);

    [UserMapping]
    public static string IpAddressToString(IPAddress src) => src.ToString();

    [UserMapping]
    public static byte[] IpAddressToBytes(IPAddress src) => src.GetAddressBytes();
}
