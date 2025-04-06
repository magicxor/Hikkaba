using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
internal static partial class SystemInfoMapper
{
    internal static partial SystemInfoViewModel ToViewModel(this SystemInfoModel model);
}
