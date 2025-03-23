using Hikkaba.Infrastructure.Models.Board;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Models.Post;

namespace Hikkaba.Infrastructure.Models.Thread;

public class ThreadAggregationSm
{
    public required ThreadPreviewSm Thread { get; set; }

    public required CategoryDashboardViewRm Category { get; set; }

    public required BoardRm Board { get; set; }

    public required IReadOnlyList<PostPreviewDto> Posts { get; set; }
}
