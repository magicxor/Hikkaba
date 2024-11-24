using System.Collections.Generic;

namespace Hikkaba.Models.Dto.Administration;

public class CategoryModeratorsDto
{
    public CategoryDto Category { get; set; }
    public IList<ModeratorDto> Moderators { get; set; }
}