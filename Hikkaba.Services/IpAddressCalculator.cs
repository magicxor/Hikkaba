using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Services
{
    public interface IIpAddressCalculator
    {
        bool IsInRange(IPAddress lowerInclusive, IPAddress upperInclusive, IPAddress address);
        bool IsInRange(string lowerInclusive, string upperInclusive, string address);
    }

    public class IpAddressCalculator : IIpAddressCalculator
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
                if ((lowerBoundary && addressBytes[i] < lowerBytes[i]) ||
                    (upperBoundary && addressBytes[i] > upperBytes[i]))
                {
                    return false;
                }

                lowerBoundary &= (addressBytes[i] == lowerBytes[i]);
                upperBoundary &= (addressBytes[i] == upperBytes[i]);
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
                _logger.LogError(e, $"Can't parse {lowerInclusive}, {upperInclusive}, {address}");
                return false;
            }
        }
    }
}
