using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Hikkaba.Paging.Models;

/// <summary>
/// Represents a result of a paged search.
/// </summary>
/// <typeparam name="T">The type of the data.</typeparam>
[PublicAPI]
[UsedImplicitly]
public class PagedResult<T>
{
    private readonly IPagingFilter _filter;

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedResult{T}"/> class.
    /// </summary>
    /// <param name="data">data to return.</param>
    /// <param name="filter">filter used to get the data.</param>
    /// <param name="totalItemCount">total amount of items fitting search criteria.</param>
    /// <exception cref="ArgumentNullException">if <paramref name="data"/> or <paramref name="filter"/> is null.</exception>
    public PagedResult(IReadOnlyList<T> data,
        IPagingFilter filter,
        int? totalItemCount = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(filter);

        Data = data;
        PageSize = filter.GetPageSize();
        SkippedItemCount = filter.GetSkipCount();
        PageNumber = filter.GetPageNumber();
        TotalItemCount = totalItemCount;
        _filter = filter;
    }

    /// <summary>
    /// Gets the requested data.
    /// </summary>
    public IReadOnlyList<T> Data { get; }

    /// <summary>
    /// Gets the requested amount of items.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets the amount of items skipped: (page - 1) * (page size).
    /// </summary>
    public int SkippedItemCount { get; }

    /// <summary>
    /// Gets the requested page number.
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Gets the total amount of pages fitting search criteria.
    /// </summary>
    public int? TotalPageCount => TotalItemCount.HasValue
        ? Convert.ToInt32(Math.Ceiling((double)TotalItemCount / PageSize))
        : null;

    /// <summary>
    /// Gets the total amount of items fitting search criteria.
    /// </summary>
    public int? TotalItemCount { get; }

    /// <summary>
    /// Gets the filter used to get the data.
    /// </summary>
    /// <returns>filter used to get the data.</returns>
    [SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "Filter field should not be serialized")]
    public IPagingFilter GetFilter()
    {
        return _filter;
    }
}
