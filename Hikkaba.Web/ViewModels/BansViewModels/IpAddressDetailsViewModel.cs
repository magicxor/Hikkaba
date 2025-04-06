using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.BansViewModels;

public class IpAddressDetailsViewModel
{
    [Display(Name = @"ASN")]
    public long? AutonomousSystemNumber { get; set; }

    [Display(Name = @"Organization")]
    public string? AutonomousSystemOrganization { get; set; }

    [Display(Name = @"Country")]
    public string? CountryIsoCode { get; set; }
}
