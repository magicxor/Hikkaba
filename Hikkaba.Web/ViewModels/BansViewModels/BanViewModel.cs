using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Enums;

namespace Hikkaba.Web.ViewModels.BansViewModels;

public sealed class BanViewModel
{
    [Display(Name = @"Id")]
    public required int Id { get; set; }

    [Display(Name = @"Is deleted")]
    public required bool IsDeleted { get; set; }

    [Display(Name = @"Created at")]
    public required DateTime CreatedAt { get; set; }

    [Display(Name = @"Modified at")]
    public required DateTime? ModifiedAt { get; set; }

    [Display(Name = @"Ends at")]
    [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm")]
    public required DateTime? EndsAt { get; set; }

    [Display(Name = @"IP address type")]
    public required IpAddressType IpAddressType { get; set; }

    [Display(Name = @"Banned IP address")]
    public required IPAddress BannedIpAddress { get; set; }

    [Display(Name = @"Banned IP range lower bound")]
    [MaxLength(Defaults.MaxIpAddressStringLength)]
    public required IPAddress? BannedCidrLowerIpAddress { get; set; }

    [Display(Name = @"Banned IP range upper bound")]
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

    [Display(Name = @"Category (if any)")]
    public string? CategoryAlias { get; set; }

    public long? RelatedThreadId { get; set; }

    [Display(Name = @"Related post id")]
    public long? RelatedPostId { get; set; }
}
