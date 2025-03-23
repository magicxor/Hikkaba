using JetBrains.Annotations;

namespace Hikkaba.Paging.Models;

/// <summary>
/// Represents a sorting filter.
/// </summary>
[PublicAPI]
public interface ISortingFilter
{
    /// <summary>
    /// Gets the order by items.
    /// </summary>
    /// <returns>order by items.</returns>
    IReadOnlyList<OrderByItem>? GetOrderBy();
}
