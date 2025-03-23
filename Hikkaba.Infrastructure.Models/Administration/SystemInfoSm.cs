using System.Runtime.InteropServices;

namespace Hikkaba.Infrastructure.Models.Administration;

public class SystemInfoSm
{
    public required string DatabaseProvider { get; set; }
    public required string FrameworkDescription { get; set; }
    public required Architecture OsArchitecture { get; set; }
    public required string OsDescription { get; set; }
    public required Architecture ProcessArchitecture { get; set; }
    public required string OsPlatform { get; set; }
    public required long MemoryUsage { get; set; }
    public required int ProcessorCount { get; set; }
    public required string UserName { get; set; }
}
