using Hikkaba.Infrastructure.Models.Board;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Models.Post;

namespace Hikkaba.Infrastructure.Models.Thread;

public class ThreadAggregationRm
{
    public required BoardRm Board { get; set; }

    public required CategoryDashboardViewRm Category { get; set; }

    public required ThreadPreviewRm Thread { get; set; }

    public required IReadOnlyList<PostViewRm> Posts { get; set; }
}
