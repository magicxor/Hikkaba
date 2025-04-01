using System.Diagnostics;

namespace Hikkaba.Infrastructure.Repositories.Telemetry;

public static class RepositoriesTelemetry
{
    public static readonly ActivitySource BoardSource = new("Hikkaba.Infrastructure.Repositories.Board");
    public static readonly ActivitySource CategorySource = new("Hikkaba.Infrastructure.Repositories.Category");
    public static readonly ActivitySource ThreadSource = new("Hikkaba.Infrastructure.Repositories.Thread");
    public static readonly ActivitySource PostSource = new("Hikkaba.Infrastructure.Repositories.Post");
    public static readonly ActivitySource BanSource = new("Hikkaba.Infrastructure.Repositories.Ban");
}
