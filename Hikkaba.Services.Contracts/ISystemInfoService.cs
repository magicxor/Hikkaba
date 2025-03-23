using Hikkaba.Infrastructure.Models.Administration;

namespace Hikkaba.Services.Contracts;

public interface ISystemInfoService
{
    public SystemInfoSm GetSystemInfo();
}
