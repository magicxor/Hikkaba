using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Hikkaba.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Application.Implementations;

public sealed class IpAddressCalculator : IIpAddressCalculator
{
    private readonly ILogger<IpAddressCalculator> _logger;

    public IpAddressCalculator(ILogger<IpAddressCalculator> logger)
    {
        _logger = logger;
    }

    public bool IsInRange(IPAddress lowerInclusive, IPAddress upperInclusive, IPAddress address)
    {
        if (lowerInclusive.AddressFamily != upperInclusive.AddressFamily)
        {
            return false;
        }

        var addressFamily = lowerInclusive.AddressFamily;
        var lowerBytes = lowerInclusive.GetAddressBytes();
        var upperBytes = upperInclusive.GetAddressBytes();

        if (address.AddressFamily != addressFamily)
        {
            return false;
        }

        var addressBytes = address.GetAddressBytes();

        bool lowerBoundary = true, upperBoundary = true;

        for (var i = 0; i < lowerBytes.Length && (lowerBoundary || upperBoundary); i++)
        {
            if ((lowerBoundary && addressBytes[i] < lowerBytes[i])
                ||
                (upperBoundary && addressBytes[i] > upperBytes[i]))
            {
                return false;
            }

            lowerBoundary &= addressBytes[i] == lowerBytes[i];
            upperBoundary &= addressBytes[i] == upperBytes[i];
        }

        return true;
    }

    public bool IsInRange(string lowerInclusive, string upperInclusive, string address)
    {
        try
        {
            return IsInRange(IPAddress.Parse(lowerInclusive), IPAddress.Parse(upperInclusive), IPAddress.Parse(address));
        }
        catch (FormatException e)
        {
            _logger.LogError(e, "Can\'t process {IsInRangeName} with arguments {LowerInclusive}, {UpperInclusive}, {Address}", nameof(IsInRange), lowerInclusive, upperInclusive, address);
            return false;
        }
    }

    public bool IsInRange(IPAddress networkAddress, short networkPrefix, IPAddress ipAddress)
    {
        return new IPNetwork(networkAddress, networkPrefix).Contains(ipAddress);
    }

    public bool IsInRange(string networkAddressStr, string ipAddressStr)
    {
        var networkAddressSpan = networkAddressStr.AsSpan();
        var slashIndex = networkAddressSpan.IndexOf('/');

        if (slashIndex == -1)
            throw new FormatException($"Invalid {nameof(networkAddressStr)} format: missing '/'.");

        if (slashIndex == 0)
            throw new FormatException($"Invalid {nameof(networkAddressStr)} format: missing network address.");

        if (slashIndex == networkAddressSpan.Length - 1)
            throw new FormatException($"Invalid {nameof(networkAddressStr)} format: missing network prefix.");

        var networkAddressPart = networkAddressSpan[..slashIndex];
        var networkPrefixPart = networkAddressSpan[(slashIndex + 1)..];

        if (!IPAddress.TryParse(networkAddressPart, out var networkAddress))
            throw new FormatException($"Invalid {nameof(networkAddressStr)} format: cannot parse IP address.");

        if (!short.TryParse(networkPrefixPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var networkPrefix))
            throw new FormatException($"Invalid {nameof(networkAddressStr)} format: cannot parse prefix.");

        if (!IPAddress.TryParse(ipAddressStr, out var ipAddress))
            throw new FormatException($"Invalid {nameof(ipAddressStr)} format: cannot parse IP address.");

        return IsInRange(networkAddress, networkPrefix, ipAddress);
    }

    /// <summary>
    /// Determines if an IP address is private (RFC 1918), loopback, or link-local.
    /// </summary>
    /// <param name="ipAddress">The IP address to check.</param>
    /// <returns>True if the IP address is private, loopback, or link-local; otherwise, false.</returns>
    public bool IsPrivate(IPAddress ipAddress)
    {
        // Handle IPv6
        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // Check standard "internal" IPv6 address types
            return ipAddress.IsIPv6LinkLocal // fe80::/10
                   || ipAddress.IsIPv6UniqueLocal // fc00::/7
                   || ipAddress.IsIPv6SiteLocal // fec0::/10 (deprecated but might exist)
                   || IPAddress.IsLoopback(ipAddress); // ::1
        }

        // Handle IPv4
        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            // Check for Loopback (127.x.x.x)
            // Note: IPAddress.IsLoopback covers 127.0.0.0/8 for IPv4 and ::1 for IPv6
            if (IPAddress.IsLoopback(ipAddress))
            {
                return true;
            }

            // Get address bytes for manual range checking
            byte[] bytes = ipAddress.GetAddressBytes();

            switch (bytes[0])
            {
                case 10: // 10.0.0.0/8    (RFC 1918)
                    return true;
                case 172: // 172.16.0.0/12 (RFC 1918)
                    return bytes[1] >= 16 && bytes[1] <= 31;
                case 192: // 192.168.0.0/16 (RFC 1918)
                    return bytes[1] == 168;
                case 169: // 169.254.0.0/16 (Link-Local / APIPA)
                    return bytes[1] == 254;
                default:
                    return false;
            }
        }

        // Unknown address type (shouldn't happen with modern IPAddress instances)
        return false;
    }
}
