using System;
using Hikkaba.Common.Dto.Base;

namespace Hikkaba.Common.Dto
{
    public class CategoryDto : BaseMutableDto
    { 
        public string Alias { get; set; }
        public string Name { get; set; }
        public bool IsHidden { get; set; }
        public int DefaultBumpLimit { get; set; }
        public bool DefaultShowThreadLocalUserHash { get; set; }

        public Guid BoardId { get; set; }
    }
}
