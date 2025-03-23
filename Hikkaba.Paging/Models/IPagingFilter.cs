using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Hikkaba.Paging.Models;

/// <summary>
/// Represents a paging filter.
/// </summary>
[PublicAPI]
public interface IPagingFilter : ISortingFilter
{
    /// <summary>
    /// Gets the amount of items to retrieve.
    /// See <see cref="M:System.Linq.Queryable.Take``1(System.Linq.IQueryable{``0},System.Int32)"/>.
    /// </summary>
    /// <returns>page size.</returns>
    [SuppressMessage("Documentation", "CA1200:Avoid using cref tags with a prefix", Justification = "The code reference must use a prefix because the referenced type is not findable")]
    [SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "This makes more sense as a method")]
    int GetPageSize();

    /// <summary>
    /// Gets the amount of items to skip (page - 1) * (page size).
    /// See <see cref="Queryable.Skip{TSource}"/>.
    /// </summary>
    /// <returns>skip count.</returns>
    int GetSkipCount();

    /// <summary>
    /// Gets the requested page number.
    /// </summary>
    /// <returns>page number.</returns>
    int GetPageNumber();
}
