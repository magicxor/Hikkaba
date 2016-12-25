using System;
using Hikkaba.Common.Dto.Base;

namespace Hikkaba.Common.Dto
{
    public class BanDto: BaseMutableDto
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string LowerIpAddress { get; set; }
        public string UpperIpAddress { get; set; }
        public string Reason { get; set; }
        public PostDto RelatedPost { get; set; }
        public CategoryDto Category { get; set; }
    }
}