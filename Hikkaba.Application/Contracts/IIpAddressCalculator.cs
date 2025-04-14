using System.Net;

namespace Hikkaba.Application.Contracts;

public interface IIpAddressCalculator
{
    bool IsInRange(IPAddress lowerInclusive, IPAddress upperInclusive, IPAddress address);
    bool IsInRange(string lowerInclusive, string upperInclusive, string address);
    bool IsPrivate(IPAddress ipAddress);
}
