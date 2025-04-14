using System.Net;

namespace Hikkaba.Infrastructure.Models.Ban;

public sealed class IpAddressInfoModel
{
    public IPAddress? NetworkIpAddress { get; set; }
    public int? NetworkPrefixLength { get; set; }
    public long? AutonomousSystemNumber { get; set; }
    public string? AutonomousSystemOrganization { get; set; }
    public string? CountryIsoCode { get; set; }
    public IPAddress? LowerIpAddress { get; set; }
    public IPAddress? UpperIpAddress { get; set; }
}
