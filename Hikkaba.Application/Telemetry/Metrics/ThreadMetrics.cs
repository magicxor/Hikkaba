using System.Diagnostics.Metrics;

namespace Hikkaba.Application.Telemetry.Metrics;

public sealed class ThreadMetrics
{
    private const string ThreadCreatedCounterName = "hikkaba.thread.created";

    private readonly Counter<int> _threadCreatedCounter;

    public ThreadMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Hikkaba.Application.Thread");
        _threadCreatedCounter = meter.CreateCounter<int>(ThreadCreatedCounterName, description: "Number of created threads");
    }

    public void ThreadCreated(long threadId, long postId, int attachmentsCount, long attachmentsSize)
    {
        _threadCreatedCounter.Add(1,
            new KeyValuePair<string, object?>("hikkaba.thread.created.thread.id", threadId),
            new KeyValuePair<string, object?>("hikkaba.thread.created.post.id", postId),
            new KeyValuePair<string, object?>("hikkaba.thread.created.attachments.count", attachmentsCount),
            new KeyValuePair<string, object?>("hikkaba.thread.created.attachments.size", attachmentsSize));
    }
}
