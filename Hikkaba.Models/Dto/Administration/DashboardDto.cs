using System.Collections.Generic;

namespace Hikkaba.Models.Dto.Administration;

public class DashboardDto
{
    public IList<CategoryModeratorsDto> CategoriesModerators { get; set; }
}