using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;

namespace Hikkaba.Application.Telemetry.Metrics;

public sealed class AuthMessageSenderMetrics
{
    private readonly Counter<int> _emailSentCounter;

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "There is no using in MS docs")]
    public AuthMessageSenderMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Hikkaba.Application.AuthMessageSender");

        _emailSentCounter = meter.CreateCounter<int>(
            name: "hikkaba.email.sent.total",
            unit: "{email}",
            description: "Total number of sent emails");
    }

    public void EmailSent()
    {
        _emailSentCounter.Add(1);
    }
}
