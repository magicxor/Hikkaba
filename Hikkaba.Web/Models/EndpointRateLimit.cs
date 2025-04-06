using System.Threading.RateLimiting;

namespace Hikkaba.Web.Models;

public sealed class EndpointRateLimit
{
    public required string Path { get; init; }
    public required string Method { get; init; }
    public required string PartitionName { get; init; }
    public required SlidingWindowRateLimiterOptions SlidingWindowOptions { get; init; }
}
