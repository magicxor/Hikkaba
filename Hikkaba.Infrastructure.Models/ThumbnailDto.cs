namespace Hikkaba.Infrastructure.Models;

public class ThumbnailDto
{
    public required Stream Image { get; set; }
    public required int Height { get; set; }
    public required int Width { get; set; }
}
