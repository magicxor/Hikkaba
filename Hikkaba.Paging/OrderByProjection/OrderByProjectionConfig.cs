using System.Collections.Concurrent;
using System.Linq.Expressions;
using Hikkaba.Paging.Models;
using JetBrains.Annotations;

namespace Hikkaba.Paging.OrderByProjection;

/// <summary>
/// Represents the global OrderBy projection configuration.
/// </summary>
[PublicAPI]
public static class OrderByProjectionConfig
{
    /// <summary>
    /// Gets the replacement rules.
    /// Matching items will be replaced with the new ones.
    /// </summary>
    public static ConcurrentDictionary<Type, ConcurrentDictionary<string, Func<OrderByItem, IReadOnlyList<OrderByItem>>>> ReplaceRules { get; } = new ();

    /// <summary>
    /// Gets the append rules.
    /// The item will be appended to the end of the OrderBy list.
    /// </summary>
    public static ConcurrentDictionary<Type, OrderByItem> AppendRules { get; } = new ();

    /// <summary>
    /// Gets the mapping rules.
    /// </summary>
    public static ConcurrentDictionary<Type, ConcurrentDictionary<string, LambdaExpression>> MappingRules { get; } = new();

    /// <summary>
    /// Clears all the rules.
    /// </summary>
    public static void Clear()
    {
        ReplaceRules.Clear();
        AppendRules.Clear();
        MappingRules.Clear();
    }

    /// <summary>
    /// Creates a new instance of <see cref="OrderByProjectionSetter{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntityMember">Entity member type.</typeparam>
    /// <returns>Instance of <see cref="OrderByProjectionSetter{TEntity}"/>.</returns>
    public static OrderByProjectionSetter<TEntityMember> ForType<TEntityMember>()
    {
        return new OrderByProjectionSetter<TEntityMember>();
    }
}
