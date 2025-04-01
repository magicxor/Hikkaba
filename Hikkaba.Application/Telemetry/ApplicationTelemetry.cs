using System.Diagnostics;

namespace Hikkaba.Application.Telemetry;

public static class ApplicationTelemetry
{
    public static readonly ActivitySource GeoIpServiceSource = new("Hikkaba.Application.GeoIpService");
    public static readonly ActivitySource AttachmentServiceSource = new("Hikkaba.Application.AttachmentService");
    public static readonly ActivitySource HashServiceSource = new("Hikkaba.Application.HashService");
    public static readonly ActivitySource HtmlUtilitiesSource = new("Hikkaba.Application.HtmlUtilities");
    public static readonly ActivitySource ThumbnailGeneratorSource = new("Hikkaba.Application.ThumbnailGenerator");
}
