using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Models.Board;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Hikkaba.Web.ViewModels.BoardViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
public static partial class SystemInfoMapper
{
    public static partial SystemInfoViewModel ToViewModel(this SystemInfoSm model);
}
