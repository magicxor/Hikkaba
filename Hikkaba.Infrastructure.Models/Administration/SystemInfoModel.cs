using System.Runtime.InteropServices;

namespace Hikkaba.Infrastructure.Models.Administration;

public sealed class SystemInfoModel
{
    public required string DatabaseProvider { get; set; }
    public required string FrameworkDescription { get; set; }
    public required Architecture OsArchitecture { get; set; }
    public required string OsDescription { get; set; }
    public required Architecture ProcessArchitecture { get; set; }
    public required string OsPlatform { get; set; }

    /// <summary>
    /// Working set.
    /// This is the amount of process memory (both private and shared) that is currently in physical random access memory (RAM).
    /// </summary>
    public required long WorkingSet64 { get; set; }

    /// <summary>
    /// Private memory size.
    /// This is the part of PagedMemorySize64 that is private to the process, meaning it cannot be shared with other processes.
    /// </summary>
    public required long PrivateMemorySize64 { get; set; }

    /// <summary>
    /// Paged memory size.
    /// This is the total amount of virtual memory committed by the process that can be paged to the disk swap file.
    /// This is the broadest category of these three, relating to committed pageable memory.
    /// It includes both the private memory of the process and shared memory (e.g., parts of DLLs that can be paged out).
    /// </summary>
    public required long PagedMemorySize64 { get; set; }

    public required long PeakWorkingSet64 { get; set; }

    public required long PeakPagedMemorySize64 { get; set; }

    public required int ProcessorCount { get; set; }

    public required string UserName { get; set; }
}
