using System.Collections.Generic;

namespace Hikkaba.Models.Dto
{
    public class ThreadPostsAggregationDto
    {
        public CategoryDto Category { get; set; }
        public ThreadDto Thread { get; set; }
        public IList<PostDto> Posts { get; set; }
    }
}