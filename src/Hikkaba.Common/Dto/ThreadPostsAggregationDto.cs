using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Common.Dto
{
    public class ThreadPostsAggregationDto
    {
        public CategoryDto Category { get; set; }
        public ThreadDto Thread { get; set; }
        public IList<PostDto> Posts { get; set; }
    }
}