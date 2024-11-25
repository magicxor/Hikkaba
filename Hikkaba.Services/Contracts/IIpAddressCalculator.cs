using System.Net;

namespace Hikkaba.Services.Contracts;

public interface IIpAddressCalculator
{
    bool IsInRange(IPAddress lowerInclusive, IPAddress upperInclusive, IPAddress address);
    bool IsInRange(string lowerInclusive, string upperInclusive, string address);
}