using JetBrains.Annotations;

namespace Hikkaba.Paging.Enums;

/// <summary>
/// Sort direction.
/// </summary>
[PublicAPI]
public enum OrderByDirection
{
    /// <summary>
    /// Ascending.
    /// </summary>
    Asc = 0,

    /// <summary>
    /// Descending.
    /// </summary>
    Desc = 1,
}
