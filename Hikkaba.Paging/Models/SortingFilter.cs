using JetBrains.Annotations;

namespace Hikkaba.Paging.Models;

/// <summary>
/// Base class for the sorting filter.
/// </summary>
[PublicAPI]
public class SortingFilter : ISortingFilter
{
    /// <summary>
    /// Gets or sets order by.
    /// </summary>
    public IReadOnlyList<OrderByItem>? OrderBy { get; set; }

    /// <inheritdoc />
    public IReadOnlyList<OrderByItem>? GetOrderBy()
    {
        return OrderBy;
    }
}
