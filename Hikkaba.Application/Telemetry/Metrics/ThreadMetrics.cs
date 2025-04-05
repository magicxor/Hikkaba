using System.Diagnostics.Metrics;
using Hikkaba.Shared.Enums;

namespace Hikkaba.Application.Telemetry.Metrics;

public sealed class ThreadMetrics
{
    private readonly Counter<int> _threadCreatedCounter;

    public ThreadMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Hikkaba.Application.Thread");

        _threadCreatedCounter = meter.CreateCounter<int>(
            name: "hikkaba.thread.created.total",
            unit: "{thread}",
            description: "Total number of created threads");
    }

    public void ThreadCreated(string categoryAlias)
    {
        _threadCreatedCounter.Add(1,
            new KeyValuePair<string, object?>(MetricTagNames.CategoryAlias, categoryAlias));
    }
}
