using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.RateLimiting;
using Hikkaba.Web.Models;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.Utils;

internal static class RateLimiterFactory
{
    public const int DefaultSegmentsPerWindow = 12;

    public const int DefaultLimit = 100;

    public static readonly TimeSpan DefaultWindow = TimeSpan.FromMinutes(1);

    public static readonly IReadOnlyList<EndpointRateLimit> DefaultEndpointRateLimits = new List<EndpointRateLimit>
    {
        new() { Path = "/api/v1/attachments", Method = HttpMethod.Get.Method, PartitionName = "attachments_GET", SlidingWindowOptions = CreateSlidingWindowOptions(3_600_000) },
        new() { Path = "/api/v1/maintenance", Method = HttpMethod.Post.Method, PartitionName = "maintenance_POST", SlidingWindowOptions = CreateSlidingWindowOptions(60) },

        new() { Path = "/Posts", Method = HttpMethod.Post.Method, PartitionName = "posts_POST", SlidingWindowOptions = CreateSlidingWindowOptions(10) },
        new() { Path = "/Posts", Method = HttpMethod.Get.Method, PartitionName = "posts_GET", SlidingWindowOptions = CreateSlidingWindowOptions(100) },

        new() { Path = "/Threads", Method = HttpMethod.Post.Method, PartitionName = "threads_POST", SlidingWindowOptions = CreateSlidingWindowOptions(6) },
        new() { Path = "/Threads", Method = HttpMethod.Get.Method, PartitionName = "threads_GET", SlidingWindowOptions = CreateSlidingWindowOptions(100) },

        new() { Path = "/Categories", Method = HttpMethod.Post.Method, PartitionName = "categories_POST", SlidingWindowOptions = CreateSlidingWindowOptions(5) },
        new() { Path = "/Categories", Method = HttpMethod.Get.Method, PartitionName = "categories_GET", SlidingWindowOptions = CreateSlidingWindowOptions(100) },
    };

    public static readonly SlidingWindowRateLimiterOptions DefaultSlidingWindowOptions = CreateSlidingWindowOptions(
        permitLimit: DefaultLimit,
        window: DefaultWindow,
        segmentsPerWindow: DefaultSegmentsPerWindow);

    public static SlidingWindowRateLimiterOptions CreateSlidingWindowOptions(
        int permitLimit,
        TimeSpan? window = null,
        int segmentsPerWindow = DefaultSegmentsPerWindow)
    {
        return new SlidingWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window ?? DefaultWindow,
            SegmentsPerWindow = segmentsPerWindow,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
        };
    }

    public static RateLimitPartition<string> CreateSlidingRateLimitPartition(
        string partitionKey,
        SlidingWindowRateLimiterOptions slidingWindowOptions)
    {
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: partitionKey,
            factory: _ => slidingWindowOptions);
    }

    public static PartitionedRateLimiter<HttpContext> CreateSlidingPerEndpointPerIpPerMinute(
        IReadOnlyList<EndpointRateLimit> endpointLimits)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            var path = httpContext.Request.Path.ToString();
            var method = httpContext.Request.Method;

            // Different limits for different paths
            foreach (var endpointLimit in endpointLimits)
            {
                if (method.Equals(endpointLimit.Method, StringComparison.Ordinal)
                    && path.StartsWith(endpointLimit.Path, StringComparison.Ordinal))
                {
                    return CreateSlidingRateLimitPartition(
                        $"{httpContext.Connection.RemoteIpAddress}-{endpointLimit.PartitionName}",
                        endpointLimit.SlidingWindowOptions);
                }
            }

            return CreateSlidingRateLimitPartition(
                $"{httpContext.Connection.RemoteIpAddress}",
                DefaultSlidingWindowOptions);
        });
    }
}
