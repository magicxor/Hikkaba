using System.Net;
using Hikkaba.Infrastructure.Models;

namespace Hikkaba.Services.Contracts;

public interface IGeoIpService
{
    IpAddressInfo GetIpAddressInfo(IPAddress ipAddress);
}
