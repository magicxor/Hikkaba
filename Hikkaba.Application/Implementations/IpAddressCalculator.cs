using System.Globalization;
using System.Net;
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
}
