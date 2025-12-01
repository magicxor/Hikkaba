using System.Diagnostics.CodeAnalysis;
using Hikkaba.Paging.Enums;
using JetBrains.Annotations;

namespace Hikkaba.Paging.Models;

/// <summary>
/// Order by.
/// </summary>
[PublicAPI]
public sealed class OrderByItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderByItem"/> class.
    /// </summary>
    public OrderByItem()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderByItem"/> class.
    /// </summary>
    /// <param name="field">Field name.</param>
    /// <param name="direction">Sort direction.</param>
    [SetsRequiredMembers]
    public OrderByItem(string field, OrderByDirection direction = OrderByDirection.Asc)
    {
        Field = field;
        Direction = direction;
    }

    /// <summary>
    /// Gets or sets field name.
    /// </summary>
    public required string Field { get; set; }

    /// <summary>
    /// Gets or sets sort direction.
    /// </summary>
    public OrderByDirection Direction { get; set; } = OrderByDirection.Asc;

    public static implicit operator OrderByItem(string fieldName)
    {
        return new OrderByItem { Field = fieldName };
    }

    public OrderByItem ToOrderByItem()
    {
        return new OrderByItem
        {
            Field = Field,
            Direction = Direction,
        };
    }
}
