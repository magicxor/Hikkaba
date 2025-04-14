using System.Net;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Infrastructure.Mappings;

[Mapper]
public static partial class IpAddressMapper
{
    [UserMapping]
    public static IPAddress StringToIpAddress(string src) => IPAddress.Parse(src);

    [UserMapping]
    public static IPAddress? StringToIpAddressNullable(string? src) => src == null ? null : IPAddress.Parse(src);

    [UserMapping]
    public static IPAddress BytesToIpAddress(byte[] src) => new(src);

    [UserMapping]
    public static IPAddress? BytesToIpAddressNullable(byte[]? src) => src == null ? null : new IPAddress(src);

    [UserMapping]
    public static string IpAddressToString(IPAddress src) => src.ToString();

    [UserMapping]
    public static string? IpAddressToStringNullable(IPAddress? src) => src?.ToString();

    [UserMapping]
    public static byte[] IpAddressToBytes(IPAddress src) => src.GetAddressBytes();

    [UserMapping]
    public static byte[]? IpAddressToBytesNullable(IPAddress? src) => src?.GetAddressBytes();
}
