using System;
using Hikkaba.Models.Dto.Base.Current;

namespace Hikkaba.Models.Dto
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
