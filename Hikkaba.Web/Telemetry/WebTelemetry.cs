using System.Diagnostics;

namespace Hikkaba.Web.Telemetry;

internal static class WebTelemetry
{
    public static readonly ActivitySource MessagePostProcessorSource = new("Hikkaba.Web.MessagePostProcessor");
}
