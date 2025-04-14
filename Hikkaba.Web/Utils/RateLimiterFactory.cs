using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.RateLimiting;
using Hikkaba.Web.Models;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.Utils;

internal static class RateLimiterFactory
{
    private const int DefaultSegmentsPerWindow = 12;

    private const int DefaultLimit = 200;

    private static readonly TimeSpan DefaultWindow = TimeSpan.FromMinutes(1);

    public static readonly IReadOnlyList<EndpointRateLimit> DefaultEndpointRateLimits =
    [
        new() { Path = "/api/v1/attachments", Method = HttpMethod.Get.Method, PartitionName = "attachments_GET", SlidingWindowOptions = CreateSlidingWindowOptions(3_600_000) },
        new() { Path = "/api/v1/maintenance", Method = HttpMethod.Post.Method, PartitionName = "maintenance_POST", SlidingWindowOptions = CreateSlidingWindowOptions(60) },

        new() { Path = "/Posts", Method = HttpMethod.Post.Method, PartitionName = "posts_POST", SlidingWindowOptions = CreateSlidingWindowOptions(10) },
        new() { Path = "/Posts", Method = HttpMethod.Get.Method, PartitionName = "posts_GET", SlidingWindowOptions = CreateSlidingWindowOptions(200) },

        new() { Path = "/Threads", Method = HttpMethod.Post.Method, PartitionName = "threads_POST", SlidingWindowOptions = CreateSlidingWindowOptions(10) },
        new() { Path = "/Threads", Method = HttpMethod.Get.Method, PartitionName = "threads_GET", SlidingWindowOptions = CreateSlidingWindowOptions(200) },

        new() { Path = "/Categories", Method = HttpMethod.Post.Method, PartitionName = "categories_POST", SlidingWindowOptions = CreateSlidingWindowOptions(15) },
        new() { Path = "/Categories", Method = HttpMethod.Get.Method, PartitionName = "categories_GET", SlidingWindowOptions = CreateSlidingWindowOptions(200) },
    ];

    private static readonly SlidingWindowRateLimiterOptions DefaultSlidingWindowOptions = CreateSlidingWindowOptions(
        permitLimit: DefaultLimit,
        window: DefaultWindow,
        segmentsPerWindow: DefaultSegmentsPerWindow);

    private static SlidingWindowRateLimiterOptions CreateSlidingWindowOptions(
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

    private static RateLimitPartition<string> CreateSlidingRateLimitPartition(
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
                    // Console.WriteLine($"ROUTE MATCHED: Rate limit for {path} {method} is {endpointLimit.SlidingWindowOptions.PermitLimit} per {endpointLimit.SlidingWindowOptions.Window.TotalSeconds} seconds");
                    return CreateSlidingRateLimitPartition(
                        $"{httpContext.Connection.RemoteIpAddress}-{endpointLimit.PartitionName}",
                        endpointLimit.SlidingWindowOptions);
                }
            }

            // Console.WriteLine($"NO MATCH FOR ROUTE: Rate limit for {path} {method} is {DefaultSlidingWindowOptions.PermitLimit} per {DefaultSlidingWindowOptions.Window.TotalSeconds} seconds");

            return CreateSlidingRateLimitPartition(
                $"{httpContext.Connection.RemoteIpAddress}",
                DefaultSlidingWindowOptions);
        });
    }
}
