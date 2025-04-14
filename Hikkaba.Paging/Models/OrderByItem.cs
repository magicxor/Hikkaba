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
    /// Gets or sets field name.
    /// </summary>
    public string Field { get; set; } = string.Empty;

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
