using System.Diagnostics;

namespace Hikkaba.Web.Telemetry;

public static class WebTelemetry
{
    public static readonly ActivitySource MessagePostProcessorSource = new("Hikkaba.Web.MessagePostProcessor");
}
