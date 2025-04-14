using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using Hikkaba.Shared.Enums;

namespace Hikkaba.Application.Telemetry.Metrics;

public sealed class PostMetrics
{
    private readonly Counter<int> _postCreatedCounter;
    private readonly Histogram<int> _attachmentsCountHistogram;
    private readonly Histogram<long> _attachmentsSizeHistogram;

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "There is no using in MS docs")]
    public PostMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Hikkaba.Application.Post");

        _postCreatedCounter = meter.CreateCounter<int>(
            name: "hikkaba.post.created.total",
            unit: "{post}",
            description: "Total number of created posts");

        _attachmentsCountHistogram = meter.CreateHistogram<int>(
            name: "hikkaba.post.attachments.count",
            unit: "{attachment}",
            description: "Distribution of the number of attachments per post");

        _attachmentsSizeHistogram = meter.CreateHistogram<long>(
            name: "hikkaba.post.attachments.size.bytes",
            unit: "By",
            description: "Distribution of the total size of attachments per post");
    }

    public void PostCreated(string categoryAlias, int attachmentsCount, long attachmentsSize)
    {
        _postCreatedCounter.Add(1,
            new KeyValuePair<string, object?>(MetricTagNames.CategoryAlias, categoryAlias));

        _attachmentsCountHistogram.Record(attachmentsCount,
            new KeyValuePair<string, object?>(MetricTagNames.CategoryAlias, categoryAlias));

        _attachmentsSizeHistogram.Record(attachmentsSize,
            new KeyValuePair<string, object?>(MetricTagNames.CategoryAlias, categoryAlias));
    }
}
