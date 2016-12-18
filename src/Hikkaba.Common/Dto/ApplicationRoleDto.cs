using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Dto.Base;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Hikkaba.Common.Dto
{
    public class ApplicationRoleDto: BaseDto
    {
        public string NormalizedName { get; set; }
    }
}
