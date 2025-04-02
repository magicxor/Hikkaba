using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Enums;

namespace Hikkaba.Web.ViewModels.BansViewModels;

public class BanViewModel
{
    [Display(Name = @"Id")]
    public required int Id { get; set; }

    [Display(Name = @"Is deleted")]
    public required bool IsDeleted { get; set; }

    [Display(Name = @"Creation date and time")]
    public required DateTime CreatedAt { get; set; }

    [Display(Name = @"Modification date and time")]
    public required DateTime? ModifiedAt { get; set; }

    [Display(Name = @"Ends at")]
    [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm")]
    public required DateTime? EndsAt { get; set; }

    [Display(Name = @"IP address type")]
    public required IpAddressType IpAddressType { get; set; }

    [Display(Name = @"Lower IP address")]
    public required IPAddress BannedIpAddress { get; set; }

    [Display(Name = @"Lower IP address")]
    [MaxLength(Defaults.MaxIpAddressStringLength)]
    public required IPAddress? BannedCidrLowerIpAddress { get; set; }

    [Display(Name = @"Upper IP address")]
    [MaxLength(Defaults.MaxIpAddressStringLength)]
    public required IPAddress? BannedCidrUpperIpAddress { get; set; }

    [Display(Name = @"Country")]
    public string? CountryIsoCode { get; set; }

    [Display(Name = @"ASN")]
    public long? AutonomousSystemNumber { get; set; }

    [Display(Name = @"Organization")]
    public string? AutonomousSystemOrganization { get; set; }

    [Display(Name = @"Reason")]
    [MaxLength(Defaults.MaxReasonLength)]
    public required string Reason { get; set; }

    public string? CategoryAlias { get; set; }

    public long? RelatedThreadId { get; set; }

    public long? RelatedPostId { get; set; }
}
