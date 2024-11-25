using System.Collections.Generic;
using Hikkaba.Models.Dto.Board;
using Hikkaba.Models.Dto.Post;

namespace Hikkaba.Models.Dto.Thread;

public class ThreadAggregationDto
{
    public ThreadDto Thread { get; set; }
    public CategoryDto Category { get; set; }
    public BoardDto Board { get; set; }
    public IList<PostDto> Posts { get; set; }
}