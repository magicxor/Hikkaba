using System.Diagnostics;
using System.Reflection;
using Hikkaba.Application.Contracts;

namespace Hikkaba.Application.Implementations;

public class VersionInfoService : IVersionInfoService
{
    public string ProductVersion { get; }

    public VersionInfoService()
    {
        var assemblyLocation = Assembly.GetEntryAssembly()?.Location;
        ProductVersion = assemblyLocation != null
            ? FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion ?? string.Empty
            : string.Empty;
    }
}
