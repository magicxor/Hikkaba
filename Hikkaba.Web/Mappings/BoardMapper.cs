using Hikkaba.Infrastructure.Models.Board;
using Hikkaba.Web.ViewModels.BoardViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
public static partial class BoardMapper
{
    public static partial BoardViewModel ToViewModel(this BoardDetailsModel model);
}
