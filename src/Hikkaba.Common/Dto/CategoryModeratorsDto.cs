using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Common.Dto
{
    public class CategoryModeratorsDto
    {
        public CategoryDto Category { get; set; }
        public IList<ApplicationUserDto> Moderators { get; set; }
    }
}
