using Hikkaba.Models.Dto.Base.Current;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Models.Dto
{
    public class CategoryDto : BaseMutableDto
    { 
        public string Alias { get; set; }
        public string Name { get; set; }
        public bool IsHidden { get; set; }
        public int DefaultBumpLimit { get; set; }
        public bool DefaultShowThreadLocalUserHash { get; set; }

        public TPrimaryKey BoardId { get; set; }
    }
}
