using System.Diagnostics;

namespace BBCodeParser.Telemetry;

public static class BBCodeParserTelemetry
{
    public static readonly ActivitySource ParserSource = new("BBCodeParser.Telemetry.Parser");
    public static readonly ActivitySource RendererSource = new("BBCodeParser.Telemetry.Renderer");
}
