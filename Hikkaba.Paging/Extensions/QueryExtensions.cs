using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Paging.OrderByProjection;
using JetBrains.Annotations;

namespace Hikkaba.Paging.Extensions;

/// <summary>
/// Extension methods for <see cref="System.Linq.IQueryable{T}"/>.
/// </summary>
[PublicAPI]
public static class QueryExtensions
{
    /// <summary>
    /// Applies order by to <paramref name="query"/>.
    /// </summary>
    /// <param name="query">query to apply order by to.</param>
    /// <param name="filter">filter containing order by.</param>
    /// <typeparam name="T">type of query.</typeparam>
    /// <returns>query with order by applied.</returns>
    /// <exception cref="ArgumentNullException">if <paramref name="query"/> or <paramref name="filter"/> is null.</exception>
    /// <exception cref="ArgumentException">if <paramref name="filter"/> contains no order by items.</exception>
    [PublicAPI]
    public static IQueryable<T> ApplyOrderBy<T>(this IQueryable<T> query,
        ISortingFilter filter)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(filter);

        var orderBy = filter.GetOrderBy();

        if (orderBy == null)
        {
            throw new ArgumentException("OrderBy is null", nameof(filter));
        }

        if (orderBy.Count == 0)
        {
            throw new ArgumentException("OrderBy contains 0 items", nameof(filter));
        }

        if (orderBy.All(o => string.IsNullOrEmpty(o.Field)))
        {
            throw new ArgumentException("OrderBy contains 0 items with non-empty field", nameof(filter));
        }

        orderBy = TransformOrderByItems<T>(orderBy);

        foreach (var orderByItem in orderBy)
        {
            if (string.IsNullOrEmpty(orderByItem.Field))
            {
                continue;
            }

            /*
             * List<T> and Array<T> implement IOrderedQueryable<T>, but they throw
             * an exception (System.IndexOutOfRangeException) if you try to
             * call .ThenBy() or .ThenByDescending() on them.
             * OrderingMethodFinder.OrderMethodExists is the only way I found to check
             * if the query is already ordered.
             * see https://stackoverflow.com/questions/8507746/determine-whether-an-iqueryablet-has-been-ordered-or-not
             */
            var isOrdered = query is IOrderedQueryable<T>
                            && query.Expression.Type == typeof(IOrderedQueryable<T>)
                            && query.Expression.OrderMethodExists();

            var orderByOverwriteKeySelector = GetOrderByOverwrite<T>(orderByItem);

            query = orderByOverwriteKeySelector == null
                ? query.ApplyOrderByMemberPathInternal(isOrdered, orderByItem.Field, orderByItem.Direction)
                : query.ApplyOrderByKeySelectorInternal(isOrdered, orderByOverwriteKeySelector, orderByItem.Direction);
        }

        return query;
    }

    /// <summary>
    /// Applies order by to <paramref name="query"/> with fallback to <paramref name="fallbackOrderByKeySelector"/>.
    /// </summary>
    /// <param name="query">query to apply order by to.</param>
    /// <param name="filter">filter containing order by.</param>
    /// <param name="fallbackOrderByKeySelector">fallback order by key selector.</param>
    /// <param name="fallbackOrderByDirection">fallback order by direction.</param>
    /// <typeparam name="T">type of query.</typeparam>
    /// <typeparam name="TFallbackOrderByKey">type of fallback order by key.</typeparam>
    /// <returns>query with order by applied.</returns>
    /// <exception cref="ArgumentNullException">if <paramref name="query"/>, <paramref name="filter"/> or <paramref name="fallbackOrderByKeySelector"/> is null.</exception>
    [PublicAPI]
    public static IQueryable<T> ApplyOrderBy<T, TFallbackOrderByKey>(this IQueryable<T> query,
        ISortingFilter filter,
        Expression<Func<T, TFallbackOrderByKey>> fallbackOrderByKeySelector,
        OrderByDirection fallbackOrderByDirection = OrderByDirection.Asc)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(fallbackOrderByKeySelector);

        var orderBy = filter.GetOrderBy();
        if (orderBy == null
            || orderBy.Count == 0
            || orderBy.All(o => string.IsNullOrEmpty(o.Field)))
        {
            return fallbackOrderByDirection == OrderByDirection.Asc
                ? query.OrderBy(fallbackOrderByKeySelector)
                : query.OrderByDescending(fallbackOrderByKeySelector);
        }
        else
        {
            return query.ApplyOrderBy(filter);
        }
    }

    /// <summary>
    /// Applies paging to <paramref name="query"/>.
    /// </summary>
    /// <param name="query">query to apply paging to.</param>
    /// <param name="filter">filter containing paging information.</param>
    /// <typeparam name="T">type of query.</typeparam>
    /// <returns>query with paging applied.</returns>
    /// <exception cref="ArgumentNullException">if <paramref name="query"/> or <paramref name="filter"/> is null.</exception>
    [PublicAPI]
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query,
        IPagingFilter filter)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(filter);

        var skip = filter.GetSkipCount();
        var take = filter.GetPageSize();

        return query
            .Skip(skip)
            .Take(take);
    }

    /// <summary>
    /// Applies order by and paging to <paramref name="query"/>.
    /// </summary>
    /// <param name="query">query to apply order by and paging to.</param>
    /// <param name="filter">filter containing order by and paging.</param>
    /// <typeparam name="T">type of query.</typeparam>
    /// <returns>query with order by and paging applied.</returns>
    /// <exception cref="ArgumentNullException">if <paramref name="query"/> or <paramref name="filter"/> is null.</exception>
    [PublicAPI]
    public static IQueryable<T> ApplyOrderByAndPaging<T>(this IQueryable<T> query,
        IPagingFilter filter)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(filter);

        return query
            .ApplyOrderBy(filter)
            .ApplyPaging(filter);
    }

    /// <summary>
    /// Applies order by and paging to <paramref name="query"/> with fallback to <paramref name="fallbackOrderByKeySelector"/>.
    /// </summary>
    /// <param name="query">query to apply order by and paging to.</param>
    /// <param name="filter">filter containing order by and paging.</param>
    /// <param name="fallbackOrderByKeySelector">fallback order by key selector.</param>
    /// <param name="fallbackOrderByDirection">fallback order by direction.</param>
    /// <typeparam name="T">type of query.</typeparam>
    /// <typeparam name="TFallbackOrderByKey">type of fallback order by key.</typeparam>
    /// <returns>query with order by and paging applied.</returns>
    /// <exception cref="ArgumentNullException">if <paramref name="query"/>, <paramref name="filter"/> or <paramref name="fallbackOrderByKeySelector"/> is null.</exception>
    [PublicAPI]
    public static IQueryable<T> ApplyOrderByAndPaging<T, TFallbackOrderByKey>(this IQueryable<T> query,
        IPagingFilter filter,
        Expression<Func<T, TFallbackOrderByKey>> fallbackOrderByKeySelector,
        OrderByDirection fallbackOrderByDirection = OrderByDirection.Asc)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(filter);

        return query
            .ApplyOrderBy(filter, fallbackOrderByKeySelector, fallbackOrderByDirection)
            .ApplyPaging(filter);
    }

    private static IReadOnlyList<OrderByItem> TransformOrderByItems<T>(IReadOnlyList<OrderByItem> existingOrderByItems)
    {
        var result = new List<OrderByItem>();

        foreach (var existingOrderByItem in existingOrderByItems)
        {
            if (OrderByProjectionConfig.ReplaceRules.TryGetValue(typeof(T), out var typeMappingRules)
                && typeMappingRules.TryGetValue(existingOrderByItem.Field, out var keySelector))
            {
                result.AddRange(keySelector(existingOrderByItem));
            }
            else
            {
                result.Add(existingOrderByItem);
            }
        }

        if (OrderByProjectionConfig.AppendRules.TryGetValue(typeof(T), out var itemToAppend)
            && !result.Exists(o => o.Field.Equals(itemToAppend.Field, StringComparison.OrdinalIgnoreCase)))
        {
            result.Add(itemToAppend);
        }

        return result.AsReadOnly();
    }

    private static IOrderedQueryable<T> ApplyOrderByMemberPathInternal<T>(
        this IQueryable<T> query,
        bool isOrdered,
        string memberPath,
        OrderByDirection direction)
    {
        return isOrdered
            ? direction == OrderByDirection.Asc
                ? ((IOrderedQueryable<T>)query).ThenBy(memberPath)
                : ((IOrderedQueryable<T>)query).ThenByDescending(memberPath)
            : direction == OrderByDirection.Asc
                ? query.OrderBy(memberPath)
                : query.OrderByDescending(memberPath);
    }

    private static IOrderedQueryable<T> ApplyOrderByTypedKeySelectorInternal<T, TKey>(
        this IQueryable<T> query,
        bool isOrdered,
        Expression<Func<T, TKey>> keySelector,
        OrderByDirection direction)
    {
        return isOrdered
            ? direction == OrderByDirection.Asc
                ? ((IOrderedQueryable<T>)query).ThenBy(keySelector)
                : ((IOrderedQueryable<T>)query).ThenByDescending(keySelector)
            : direction == OrderByDirection.Asc
                ? query.OrderBy(keySelector)
                : query.OrderByDescending(keySelector);
    }

    [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "This is the only way to call a generic method with a generic type parameter that is not known at compile time")]
    private static IOrderedQueryable<T> ApplyOrderByAnyKeySelectorInternal<T>(
        this IQueryable<T> query,
        bool isOrdered,
        LambdaExpression keySelector,
        OrderByDirection direction)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(keySelector);

        // We'll create a method to invoke the strongly-typed version of our logic
        var method = typeof(QueryExtensions)
            .GetMethod(nameof(ApplyOrderByTypedKeySelectorInternal), BindingFlags.NonPublic | BindingFlags.Static)
                     ?? throw new InvalidOperationException($"Could not find method {nameof(ApplyOrderByTypedKeySelectorInternal)}");

        try
        {
            // Using MakeGenericMethod to bind our generic type at runtime
            var genericMethod = method.MakeGenericMethod(typeof(T), keySelector.ReturnType);
            var result = genericMethod.Invoke(null, new object[] { query, isOrdered, keySelector, direction })
                         ?? throw new InvalidOperationException($"Could not invoke method {nameof(ApplyOrderByTypedKeySelectorInternal)}");

            return (IOrderedQueryable<T>)result;
        }
        catch
        {
            throw new ArgumentException($"Invalid key selector type: {keySelector.GetType().FullName}", nameof(keySelector));
        }
    }

    private static IOrderedQueryable<T> ApplyOrderByKeySelectorInternal<T>(
        this IQueryable<T> query,
        bool isOrdered,
        LambdaExpression keySelector,
        OrderByDirection direction)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(keySelector);

        return keySelector switch
        {
            Expression<Func<T, bool>> keySelectorBool => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorBool, direction),
            Expression<Func<T, bool?>> keySelectorBoolNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorBoolNullable, direction),
            Expression<Func<T, byte>> keySelectorByte => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorByte, direction),
            Expression<Func<T, byte?>> keySelectorByteNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorByteNullable, direction),
            Expression<Func<T, sbyte>> keySelectorSbyte => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorSbyte, direction),
            Expression<Func<T, sbyte?>> keySelectorSbyteNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorSbyteNullable, direction),
            Expression<Func<T, short>> keySelectorShort => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorShort, direction),
            Expression<Func<T, short?>> keySelectorShortNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorShortNullable, direction),
            Expression<Func<T, int>> keySelectorInt => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorInt, direction),
            Expression<Func<T, int?>> keySelectorIntNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorIntNullable, direction),
            Expression<Func<T, long>> keySelectorLong => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorLong, direction),
            Expression<Func<T, long?>> keySelectorLongNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorLongNullable, direction),
            Expression<Func<T, float>> keySelectorFloat => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorFloat, direction),
            Expression<Func<T, float?>> keySelectorFloatNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorFloatNullable, direction),
            Expression<Func<T, double>> keySelectorDouble => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorDouble, direction),
            Expression<Func<T, double?>> keySelectorDoubleNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorDoubleNullable, direction),
            Expression<Func<T, decimal>> keySelectorDecimal => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorDecimal, direction),
            Expression<Func<T, decimal?>> keySelectorDecimalNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorDecimalNullable, direction),
            Expression<Func<T, ushort>> keySelectorUshort => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorUshort, direction),
            Expression<Func<T, ushort?>> keySelectorUshortNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorUshortNullable, direction),
            Expression<Func<T, uint>> keySelectorUint => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorUint, direction),
            Expression<Func<T, uint?>> keySelectorUintNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorUintNullable, direction),
            Expression<Func<T, ulong>> keySelectorUlong => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorUlong, direction),
            Expression<Func<T, ulong?>> keySelectorUlongNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorUlongNullable, direction),
            Expression<Func<T, DateTime>> keySelectorDateTime => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorDateTime, direction),
            Expression<Func<T, DateTime?>> keySelectorDateTimeNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorDateTimeNullable, direction),
            Expression<Func<T, DateTimeOffset>> keySelectorDateTimeOffset => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorDateTimeOffset, direction),
            Expression<Func<T, DateTimeOffset?>> keySelectorDateTimeOffsetNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorDateTimeOffsetNullable, direction),
            Expression<Func<T, TimeSpan>> keySelectorTimeSpan => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorTimeSpan, direction),
            Expression<Func<T, TimeSpan?>> keySelectorTimeSpanNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorTimeSpanNullable, direction),
            Expression<Func<T, DateOnly>> keySelectorDateOnly => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorDateOnly, direction),
            Expression<Func<T, DateOnly?>> keySelectorDateOnlyNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorDateOnlyNullable, direction),
            Expression<Func<T, TimeOnly>> keySelectorTimeOnly => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorTimeOnly, direction),
            Expression<Func<T, TimeOnly?>> keySelectorTimeOnlyNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorTimeOnlyNullable, direction),
            Expression<Func<T, char>> keySelectorChar => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorChar, direction),
            Expression<Func<T, char?>> keySelectorCharNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorCharNullable, direction),
            Expression<Func<T, string>> keySelectorString => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorString, direction),
            Expression<Func<T, Guid>> keySelectorGuid => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorGuid, direction),
            Expression<Func<T, Guid?>> keySelectorGuidNullable => query.ApplyOrderByTypedKeySelectorInternal(isOrdered, keySelectorGuidNullable, direction),
            { Body: not null } when keySelector.GetType() is { IsGenericType: true } keySelectorType
                                    && keySelectorType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(Func<,>)
                => query.ApplyOrderByAnyKeySelectorInternal(isOrdered, keySelector, direction),
            _ => throw new ArgumentException($"Invalid key selector type: {keySelector.GetType().FullName}", nameof(keySelector)),
        };
    }

    private static LambdaExpression? GetOrderByOverwrite<T>(OrderByItem orderByItem)
    {
        ArgumentNullException.ThrowIfNull(orderByItem);

        return !OrderByProjectionConfig.MappingRules.TryGetValue(typeof(T), out var typeMappingRules)
            ? null
            : typeMappingRules.GetValueOrDefault(orderByItem.Field);
    }

    private static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query,
        string propertyName,
        IComparer<object>? comparer = null)
    {
        return CallOrderedQueryable(query, "OrderBy", propertyName, comparer);
    }

    private static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query,
        string propertyName,
        IComparer<object>? comparer = null)
    {
        return CallOrderedQueryable(query, "OrderByDescending", propertyName, comparer);
    }

    private static IOrderedQueryable<T> ThenBy<T>(this IQueryable<T> query,
        string propertyName,
        IComparer<object>? comparer = null)
    {
        return CallOrderedQueryable(query, "ThenBy", propertyName, comparer);
    }

    private static IOrderedQueryable<T> ThenByDescending<T>(this IQueryable<T> query,
        string propertyName,
        IComparer<object>? comparer = null)
    {
        return CallOrderedQueryable(query, "ThenByDescending", propertyName, comparer);
    }

    /// <summary>
    /// Builds the Queryable functions using a TSource property name.
    /// </summary>
    private static IOrderedQueryable<T> CallOrderedQueryable<T>(this IQueryable<T> query,
        string methodName,
        string propertyName,
        IComparer<object>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrEmpty(methodName))
        {
            throw new ArgumentException("Argument is null or empty", nameof(methodName));
        }

        if (string.IsNullOrEmpty(propertyName))
        {
            throw new ArgumentException("Argument is null or empty", nameof(propertyName));
        }

        var param = Expression.Parameter(typeof(T), "x");

        var body = propertyName.Split('.').Aggregate<string, Expression>(param, Expression.PropertyOrField);

        return comparer != null
            ? (IOrderedQueryable<T>)query.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new[] { typeof(T), body.Type },
                    query.Expression,
                    Expression.Lambda(body, param),
                    Expression.Constant(comparer)
                )
            )
            : (IOrderedQueryable<T>)query.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new[] { typeof(T), body.Type },
                    query.Expression,
                    Expression.Lambda(body, param)
                )
            );
    }
}
