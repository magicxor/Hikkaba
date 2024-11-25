using Hikkaba.Models.Dto.Post;

namespace Hikkaba.Models.Dto.Thread;

public class ThreadPostCreateDto
{
    public CategoryDto Category { get; set; }
    public ThreadDto Thread { get; set; }
    public PostDto Post { get; set; }
}