using Hikkaba.Models.Dto.Administration;

namespace Hikkaba.Services.Contracts;

public interface ISystemInfoService
{
    public SystemInfoDto GetSystemInfo();
}