using System.Collections.Generic;

namespace Hikkaba.Common.Dto.Administration
{
    public class CategoryModeratorsDto
    {
        public CategoryDto Category { get; set; }
        public IList<ModeratorDto> Moderators { get; set; }
    }
}