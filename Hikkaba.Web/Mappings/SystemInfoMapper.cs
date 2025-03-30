using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
public static partial class SystemInfoMapper
{
    public static partial SystemInfoViewModel ToViewModel(this SystemInfoModel model);
}
