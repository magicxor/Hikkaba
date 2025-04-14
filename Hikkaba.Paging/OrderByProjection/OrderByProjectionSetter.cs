using System.Collections.Concurrent;
using System.Linq.Expressions;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using JetBrains.Annotations;

namespace Hikkaba.Paging.OrderByProjection;

/// <summary>
/// Sets the OrderBy projection for the entity.
/// </summary>
/// <typeparam name="TEntity">the entity type.</typeparam>
[PublicAPI]
public sealed class OrderByProjectionSetter<TEntity>
{
    /// <summary>
    /// Replaces the OrderBy item with the new ones.
    /// </summary>
    /// <param name="fieldPath">the field path.</param>
    /// <param name="orderByItemReplacer">the OrderBy item replacer.</param>
    /// <returns>Instance of <see cref="OrderByProjectionSetter{TEntity}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldPath"/> is null.</exception>
    public OrderByProjectionSetter<TEntity> Replace(
        string fieldPath,
        Func<OrderByItem, IReadOnlyList<OrderByItem>> orderByItemReplacer)
    {
        ArgumentNullException.ThrowIfNull(fieldPath);

        OrderByProjectionConfig.ReplaceRules.TryAdd(typeof(TEntity), new ConcurrentDictionary<string, Func<OrderByItem, IReadOnlyList<OrderByItem>>>(StringComparer.OrdinalIgnoreCase));
        OrderByProjectionConfig.ReplaceRules[typeof(TEntity)].TryAdd(fieldPath, orderByItemReplacer);

        return this;
    }

    /// <summary>
    /// Replaces the OrderBy item with the new ones.
    /// </summary>
    /// <param name="fieldPath">the field path.</param>
    /// <param name="orderByItemReplacements">the OrderBy item replacements.</param>
    /// <returns>Instance of <see cref="OrderByProjectionSetter{TEntity}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldPath"/> is null.</exception>
    public OrderByProjectionSetter<TEntity> Replace(
        string fieldPath,
        params string[] orderByItemReplacements)
    {
        ArgumentNullException.ThrowIfNull(fieldPath);

        OrderByProjectionConfig.ReplaceRules.TryAdd(typeof(TEntity), new ConcurrentDictionary<string, Func<OrderByItem, IReadOnlyList<OrderByItem>>>(StringComparer.OrdinalIgnoreCase));
        OrderByProjectionConfig.ReplaceRules[typeof(TEntity)].TryAdd(fieldPath,
            orderByItem => orderByItemReplacements.Select(replacement => new OrderByItem
                {
                    Field = replacement,
                    Direction = orderByItem.Direction,
                })
                .ToList()
                .AsReadOnly());

        return this;
    }

    /// <summary>
    /// Appends the OrderBy item to the end of the OrderBy list.
    /// </summary>
    /// <param name="fieldPath">the field path.</param>
    /// <param name="orderByDirection">the OrderBy direction.</param>
    /// <returns>Instance of <see cref="OrderByProjectionSetter{TEntity}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldPath"/> is null.</exception>
    public OrderByProjectionSetter<TEntity> Append(
        string fieldPath,
        OrderByDirection orderByDirection)
    {
        ArgumentNullException.ThrowIfNull(fieldPath);

        OrderByProjectionConfig.AppendRules.TryAdd(typeof(TEntity), new OrderByItem
        {
            Field = fieldPath,
            Direction = orderByDirection,
        });

        return this;
    }

    /// <summary>
    /// Maps the field to the entity key selector.
    /// </summary>
    /// <param name="fieldPath">the field path.</param>
    /// <param name="orderByKeySelector">the entity key selector.</param>
    /// <typeparam name="TEntityMember">the entity member type.</typeparam>
    /// <returns>Instance of <see cref="OrderByProjectionSetter{TEntity}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldPath"/> is null.</exception>
    public OrderByProjectionSetter<TEntity> Map<TEntityMember>(
        string fieldPath,
        Expression<Func<TEntity, TEntityMember>> orderByKeySelector)
    {
        ArgumentNullException.ThrowIfNull(fieldPath);

        OrderByProjectionConfig.MappingRules.TryAdd(typeof(TEntity), new ConcurrentDictionary<string, LambdaExpression>(StringComparer.OrdinalIgnoreCase));
        OrderByProjectionConfig.MappingRules[typeof(TEntity)].TryAdd(fieldPath, orderByKeySelector);

        return this;
    }
}
