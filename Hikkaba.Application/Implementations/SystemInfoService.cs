using System.Diagnostics;
using System.Runtime.InteropServices;
using Hikkaba.Application.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Administration;

namespace Hikkaba.Application.Implementations;

public sealed class SystemInfoService
    : ISystemInfoService
{
    private readonly ApplicationDbContext _applicationDbContext;

    public SystemInfoService(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    private string GetOsPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return nameof(OSPlatform.Windows);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return nameof(OSPlatform.Linux);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return nameof(OSPlatform.OSX);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            return nameof(OSPlatform.FreeBSD);
        }
        else
        {
            return "Unknown";
        }
    }

    public SystemInfoModel GetSystemInfo()
    {
        var process = Process.GetCurrentProcess();

        return new SystemInfoModel
        {
            DatabaseProvider = _applicationDbContext.Database.ProviderName ?? string.Empty,
            FrameworkDescription = RuntimeInformation.FrameworkDescription,
            OsArchitecture = RuntimeInformation.OSArchitecture,
            OsDescription = RuntimeInformation.OSDescription,
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture,
            OsPlatform = GetOsPlatform(),
            WorkingSet64 = process.WorkingSet64,
            PrivateMemorySize64 = process.PrivateMemorySize64,
            PagedMemorySize64 = process.PagedMemorySize64,
            PeakWorkingSet64 = process.PeakWorkingSet64,
            PeakPagedMemorySize64 = process.PeakPagedMemorySize64,
            ProcessorCount = Environment.ProcessorCount,
            UserName = Environment.UserName,
        };
    }
}
