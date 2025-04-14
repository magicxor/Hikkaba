using Hikkaba.Paging.Constants;
using JetBrains.Annotations;

namespace Hikkaba.Paging.Models;

/// <summary>
/// Base class for the paging filter that uses PageNumber and PageSize.
/// </summary>
[PublicAPI]
public class PageBasedPagingFilter : IPagingFilter
{
    /// <summary>
    /// Gets or sets page size.
    /// </summary>
    public int PageSize { get; set; } = Defaults.PageSize;

    /// <summary>
    /// Gets or sets page number.
    /// </summary>
    public int PageNumber { get; set; } = Defaults.PageNumber;

    /// <summary>
    /// Gets or sets order by.
    /// </summary>
    public IReadOnlyList<OrderByItem>? OrderBy { get; set; }

    /// <inheritdoc />
    public int GetPageSize()
    {
        return PageSize;
    }

    /// <inheritdoc />
    public int GetSkipCount()
    {
        return (PageNumber - 1) * PageSize;
    }

    /// <inheritdoc />
    public int GetPageNumber()
    {
        return PageNumber;
    }

    /// <inheritdoc />
    public IReadOnlyList<OrderByItem>? GetOrderBy()
    {
        return OrderBy;
    }
}
