using System.Net;
using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Models.Ban;

public sealed class BanPagingFilter : PageBasedPagingFilter
{
    public bool IncludeDeleted { get; set; }
    public DateTime? CreatedNotBefore { get; set; }
    public DateTime? CreatedNotAfter { get; set; }
    public DateTime? EndsNotBefore { get; set; }
    public DateTime? EndsNotAfter { get; set; }
    public IPAddress? IpAddress { get; set; }
    public string? CountryIsoCode { get; set; }
    public int? AutonomousSystemNumber { get; set; }
    public string? AutonomousSystemOrganization { get; set; }
    public long? RelatedPostId { get; set; }
    public int? CategoryId { get; set; }
}
