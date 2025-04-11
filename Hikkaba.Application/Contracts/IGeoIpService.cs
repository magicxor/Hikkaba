using System.Net;
using Hikkaba.Infrastructure.Models.Ban;

namespace Hikkaba.Application.Contracts;

public interface IGeoIpService
{
    IpAddressInfoModel GetIpAddressInfo(IPAddress ipAddress);
}
