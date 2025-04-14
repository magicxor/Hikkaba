using System.Diagnostics.CodeAnalysis;
using Hikkaba.Paging.Constants;
using JetBrains.Annotations;

namespace Hikkaba.Paging.Models;

/// <summary>
/// Base class for the paging filter that uses SkipCount and PageSize.
/// </summary>
[PublicAPI]
public class SkipBasedPagingFilter : IPagingFilter
{
    /// <summary>
    /// Gets or sets page size.
    /// </summary>
    public int PageSize { get; set; } = Defaults.PageSize;

    /// <summary>
    /// Gets or sets skip count.
    /// </summary>
    public int SkipCount { get; set; } = Defaults.SkipCount;

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
        return SkipCount;
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "ArrangeRedundantParentheses", Justification = "SA1407: Arithmetic expressions should declare precedence")]
    public int GetPageNumber()
    {
        return (SkipCount / PageSize) + 1;
    }

    /// <inheritdoc />
    public IReadOnlyList<OrderByItem>? GetOrderBy()
    {
        return OrderBy;
    }
}
