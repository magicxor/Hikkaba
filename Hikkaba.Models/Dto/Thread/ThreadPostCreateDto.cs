namespace Hikkaba.Models.Dto;

public class ThreadPostCreateDto
{
    public CategoryDto Category { get; set; }
    public ThreadDto Thread { get; set; }
    public PostDto Post { get; set; }
}