using Hikkaba.Infrastructure.Models.Administration;

namespace Hikkaba.Application.Contracts;

public interface ISystemInfoService
{
    public SystemInfoModel GetSystemInfo();
}
