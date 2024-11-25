using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Hikkaba.Data.Context;
using Hikkaba.Models.Dto.Administration;
using Hikkaba.Services.Contracts;

namespace Hikkaba.Services.Implementations;

public class SystemInfoService: ISystemInfoService
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

    public SystemInfoDto GetSystemInfo()
    {
        return new SystemInfoDto
        {
            DatabaseProvider = _applicationDbContext.Database.ProviderName,
            FrameworkDescription = RuntimeInformation.FrameworkDescription,
            OsArchitecture = RuntimeInformation.OSArchitecture,
            OsDescription = RuntimeInformation.OSDescription,
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture,
            OsPlatform = GetOsPlatform(),
            MemoryUsage = Process.GetCurrentProcess().WorkingSet64,
            ProcessorCount = Environment.ProcessorCount,
            UserName = Environment.UserName,
        };
    }
}