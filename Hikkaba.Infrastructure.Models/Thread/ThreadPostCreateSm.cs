using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Models.Post;

namespace Hikkaba.Infrastructure.Models.Thread;

public class ThreadPostCreateSm
{
    public required CategoryDto Category { get; set; }
    public required ThreadEditSm Thread { get; set; }
    public required PostDto Post { get; set; }
}
