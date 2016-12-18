using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Dto.Base;

namespace Hikkaba.Common.Dto
{
    public class ApplicationUserDto: BaseDto
    {
        public bool IsDeleted { get; set; }
        public int AccessFailedCount { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string NormalizedEmail { get; set; }
        public string NormalizedUserName { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }
}