using System.Diagnostics.Metrics;

namespace Hikkaba.Application.Telemetry.Metrics;

public sealed class PostMetrics
{
    private const string PostCreatedCounterName = "hikkaba.post.created";

    private readonly Counter<int> _postCreatedCounter;

    public PostMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Hikkaba.Application.Post");
        _postCreatedCounter = meter.CreateCounter<int>(PostCreatedCounterName, description: "Number of created posts");
    }

    public void PostCreated(long threadId, long postId, int attachmentsCount, long attachmentsSize)
    {
        _postCreatedCounter.Add(1,
            new KeyValuePair<string, object?>("hikkaba.post.created.thread.id", threadId),
            new KeyValuePair<string, object?>("hikkaba.post.created.post.id", postId),
            new KeyValuePair<string, object?>("hikkaba.post.created.attachments.count", attachmentsCount),
            new KeyValuePair<string, object?>("hikkaba.post.created.attachments.size", attachmentsSize));
    }
}
