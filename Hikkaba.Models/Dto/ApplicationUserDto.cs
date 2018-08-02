using System;
using Hikkaba.Models.Dto.Base.Current;

namespace Hikkaba.Models.Dto
{
    public class ApplicationUserDto: BaseDto
    {
        public bool IsDeleted { get; set; }
        public int AccessFailedCount { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }
}