using System.Collections.Generic;

namespace Hikkaba.Models.Dto;

public class ThreadAggregationDto
{
    public ThreadDto Thread { get; set; }
    public CategoryDto Category { get; set; }
    public BoardDto Board { get; set; }
    public IList<PostDto> Posts { get; set; }
}