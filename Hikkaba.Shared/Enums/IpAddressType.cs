using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Shared.Enums;

public enum IpAddressType
{
    [Display(Name = "Unknown")]
    Unknown = 0,

    [Display(Name = "IPv4")]
    IpV4 = 1,

    [Display(Name = "IPv6")]
    IpV6 = 2,
}
